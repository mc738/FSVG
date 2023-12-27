namespace FSVG.Diagrams.Common.Layout

// No warning needed for use of DiagramNode and Diagram, internal usage.
#nowarn "10001"

open System
open FSVG.Diagrams.Common.Shared
open FSVG.Diagrams.Common.Shared.Rendering

[<RequireQualifiedAccess>]
module LayeredGraphDrawing =

    open FSVG.Diagrams.Common.Shared

    // Implementation based on https://en.wikipedia.org/wiki/Layered_graph_drawing

    // https://www.baeldung.com/cs/graph-auto-layout-algorithm

    type Settings =
        {
            NormalizeNodes: bool
            /// <summary>
            /// A optional node limit. This can prevent process excess size for performance reasons.
            /// </summary>
            NodeLimit: int option
            StringComparison: StringComparison
            StrictMode: bool
            /// <summary>
            /// A function for determining the preferred order of a node.
            /// If set to "None" the node's index in the the initial list will be used.
            /// </summary>
            PreferredOrderHandler: (Definitions.DiagramNode -> int) option
        }

    type Parameters =
        { Nodes: Definitions.DiagramNode list
          Settings: Settings }

    /// <summary>
    /// An internal node, this reverses the connections to store a record of
    /// </summary>
    type InternalNode =
        { Node: Definitions.DiagramNode
          PreferredOrder: int
          ConnectionsFrom: string list }

    let createInternalNodes (parameters: Parameters) =
        // PERFORMANCE Could this be optimized?
        parameters.Nodes
        |> List.mapi (fun i n ->

            { Node = n
              PreferredOrder =
                match parameters.Settings.PreferredOrderHandler with
                | Some poh -> poh n
                | None -> i
              ConnectionsFrom =
                parameters.Nodes
                |> List.filter (fun on ->
                    on.Connections
                    |> List.exists (fun nc -> nc.ToId.Equals(n.Id, parameters.Settings.StringComparison)))
                |> List.map (fun on -> on.Id) })

    type NodeLayer =
        { Level: int; Nodes: InternalNode list }

    type VerificationResultItem = { FromNode: string; ToNode: string }

    type VerificationResults =
        { Successes: VerificationResultItem list
          CyclicReferences: VerificationResultItem list
          NodesNotFound: VerificationResultItem list }

    let contains (comparison: StringComparison) (value: string) (values: string list) =
        values |> List.exists (fun v -> v.Equals(value, comparison))

    let verifyInternalNodes (parameters: Parameters) (nodes: InternalNode list) =
        // For go FP for the sake of internal usage.
        // This are oversize, but in practice that shouldn't matter,
        // this is just a optimization to make the verification process a bit quicker/less resource intensive.
        // This uses nodes.Length / 2 for the second 2 because in general there should be less of these.

        let successes = ResizeArray<VerificationResultItem>(nodes.Length)
        let cyclicReferences = ResizeArray<VerificationResultItem>(nodes.Length / 2)
        let nodesNotFound = ResizeArray<VerificationResultItem>(nodes.Length / 2)

        let nodesMap = nodes |> List.map (fun n -> n.Node.Id, n) |> Map.ofList

        nodes
        |> List.iter (fun n ->
            n.ConnectionsFrom
            |> List.iter (fun ncf ->
                match nodesMap.TryFind ncf with
                | Some fn ->
                    match fn.ConnectionsFrom |> contains parameters.Settings.StringComparison n.Node.Id with
                    | true ->
                        { FromNode = fn.Node.Id
                          ToNode = n.Node.Id }
                        |> cyclicReferences.Add
                    | false ->
                        { FromNode = fn.Node.Id
                          ToNode = n.Node.Id }
                        |> successes.Add
                | None -> { FromNode = ncf; ToNode = n.Node.Id } |> nodesNotFound.Add))

        { Successes = successes |> List.ofSeq
          CyclicReferences = cyclicReferences |> List.ofSeq
          NodesNotFound = nodesNotFound |> List.ofSeq }

    let createLayers (parameters: Parameters) (nodes: InternalNode list) =
        // With layers it is important to remember a node CAN have connections to higher later.
        // This might be from the following:
        // A -> B
        // B -> C
        // C -> A
        //
        // In this case layers should be like:
        // 0: A
        // 1: B
        // 2: C
        //
        // The render will deal with creating the connection back to layer 0.
        // This does mean however the logic here needs to smart enough to place A in the top layer,
        // even if it is presented C,B,A etc..
        //
        // To handle this each nodes connections should be "back tracked" to find multiple cycling references.
        // So for A the logic will be:
        // * C connects to A
        // * B connects to C
        // * A connects to B
        //
        // However we will still need to rely on the order which the nodes are presented or else it will be impossible
        // which comes first.
        // For example A, B and C will all appear to have the same logic.
        //
        // The introduction of the `PreferredOrder` field in the not InternalNode type is meant to handle this.
        // It basically means if A has a preferred order of 0 and B of 1,
        // then A should strive to be in a higher layer B.
        //
        // Currently it is just set to the nodes index in the initial list but this could be customised.

        let nodeOrderMap =
            nodes |> List.map (fun n -> n.Node.Id, n.PreferredOrder) |> Map.ofList

        // Step 1 - create layers. This is basically handled by ordered on the preferred other.
        // The first round is not meant to be perfect but get nodes into vaguely the right layered.
        let orderedNodes = nodes |> List.sortBy (fun n -> n.PreferredOrder)


        // Step 2 - reduce layers. This is achieved by working through all the nodes and seeing how high up in the layers they can be placed.
        // A node with no from connections or from connections with a higher preferred order will be in top layered etc.
        // A node with from connections in the top layer will be in the second layer etc.
        // However because not a nodes will have a layer at first this to be done in rounds.

        let (topLevel, remaining) =
            nodes
            |> List.partition (fun n ->
                // Check if the node has no connections coming to it or if there are some,
                // they have a higher preferred order.
                // TODO this needs testing.
                n.ConnectionsFrom.IsEmpty
                || n.ConnectionsFrom
                   |> List.exists (fun cf ->
                       nodeOrderMap.TryFind cf
                       |> Option.map (fun cfo -> cfo < n.PreferredOrder)
                       |> Option.defaultValue false)
                   |> not)

        let rec buildLayers (layers: NodeLayer list) (remaining: InternalNode list) =
            // There should always be one layer.
            let prevLayer = layers.Head

            let (layerNodes, remaining) =
                remaining
                |> List.partition (fun n ->
                    prevLayer.Nodes
                    |> List.exists (fun pn -> n.ConnectionsFrom |> List.contains pn.Node.Id)
                    || n.ConnectionsFrom
                       |> List.exists (fun cf ->
                           nodeOrderMap.TryFind cf
                           |> Option.map (fun cfo -> cfo < n.PreferredOrder)
                           |> Option.defaultValue false)
                       |> not)

            let layer =
                { Nodes = layerNodes
                  Level = prevLayer.Level + 1 }

            match remaining.IsEmpty with
            | true -> layer :: layers |> List.rev
            | false -> buildLayers (layer :: layers) remaining

        buildLayers [ { Level = 0; Nodes = topLevel } ] remaining

    let prepareRendering (parameters: Parameters) (nodes: NodeLayer list) =
        // Use grid settings

        // For the rows we use the layers.
        // The columns are a little bit trickier.
        // These need to be be in a way that makes sense.
        // Some examples:
        // 1.
        //    A
        //    |\
        //    | \
        //   /\ /
        //  B  C
        //
        // C needs to go to the far column because it links back to A
        // We also need an odd number of total columns (3 in this case).
        // So A can be in column index 1, B in column index 0 and C in column index 2.
        // OR
        // A can be positioned in in column index 0.5 (half way between 0 and 1)
        // Should there be a setting for this?
        //
        // 2.
        //    A
        //    |\
        //    | \
        //   /\ /
        //  B  C
        //     |
        //     D
        //
        //    A
        //    |\
        //    | \
        //   /\ /
        //  B  C
        //  | /
        //  D
        //
        // Prioritise keeping connected nodes in line with their LEFT MOST connection (where possible).
        //
        // 3.
        //
        //    A
        //    |\
        //    | \
        //   /\ /
        //  B  C
        //  | /|
        //  D  E
        //
        // 4.
        //
        //    A
        //    |\
        //    | \
        //   /\ /
        //  B  C
        //  |\/|
        //  |/\|
        //  D  E
        //
        // Preferred ordering might still be needed. In this case both B and C connect to D and E,
        // so we need a way to determine the best order.
        //
        // 4.
        //
        //    A
        //    |\
        //    | \
        //   /\ /
        //  B  C
        //  |  |
        //     E -- D
        //
        // Side connections - here D will be layer 1 but is put to the side
        //
        // Calculations:
        // |x|                  = n OR n + (n - 1)
        // |x| |x|              = n + (n - 1)
        // |x| |x| |x|          = n + (n - 1)
        // |x| |x| |x| |x|      = n + (n - 1)
        // |x| |x| |x| |x| |x|  = n + (n - 1)
        //
        // | | | |x| | | | -> left offset = total (4) - curr (1) = 3
        // | | |x| |x| | | -> left offset = total (4) - curr (2) = 2
        // | |x| |x| |x| | -> left offset = total (4) - curr (3) = 1
        // |x| |x| |x| |x|

        let getColumnCount (length: int) = length + (length - 1)

        let getColumnIndex (columnCount: int) (currColumnCount: int) (i: int) =
            match currColumnCount = columnCount with
            | true -> i + i 
            | false ->
                let leftOffset = columnCount - currColumnCount
                leftOffset + i + i

        let maxColumns =
            nodes
            |> List.maxBy (fun ns -> ns.Nodes.Length)
            |> fun ns -> getColumnCount ns.Nodes.Length

        let createRow (layer: NodeLayer) (prevRow: GridRow option) =
            match prevRow with
            | Some pr ->
                
                
                
                ()
            | None ->
                ()
            
        
        ({ Rows =
            nodes
            |> List.mapi (fun i ns ->
                ({ Order = ns.Level
                   Height = RenderingUnit.Fixed 100. // TODO calculate
                   Nodes =
                     ns.Nodes
                     |> List.map (fun n ->
                         ({ Node = n.Node
                            Width = RenderingUnit.Fixed 100. // TODO calculate
                            Column = getColumnIndex maxColumns ns.Nodes.Length i }
                         : Rendering.GridNode)) }
                : Rendering.GridRow)) }
        : Rendering.GridRendererSettings)


    let handle (parameters: Parameters) =

        // First create a directed acyclic graph
        // This also needs to verify data to make sure cycles don't exist.
        // Where the they are used `TwoWay` should be used instead.

        // Next create layers, the goal is to minimise the number of layers
        // and limit the number of that span multiple layers

        // Then we can start working out placement within layers and connections

        // Pre - create and verify internal nodes.
        let internalNodes = createInternalNodes parameters

        let verificationResults = verifyInternalNodes parameters internalNodes

        match parameters.Settings.StrictMode with
        | true ->
            match verificationResults.CyclicReferences.Length > 0, verificationResults.NodesNotFound.Length > 0 with
            | true, _ -> Error ""
            | _, true -> Error ""
            | false, false ->


                Ok()
        | false ->

            failwith "Non strict mode to be implemented."

    ()
