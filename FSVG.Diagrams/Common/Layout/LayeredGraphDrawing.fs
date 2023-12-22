namespace FSVG.Diagrams.Common.Layout

// No warning needed for use of DiagramNode and Diagram, internal usage.
#nowarn "10001"

open System
open FSVG.Diagrams.Common.Shared

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

    type Layer =
        { Nodes: Definitions.DiagramNode
          Order: int }


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

    let createLayers (parameters: Parameters) =
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
       
         
       
        
        
        
        let rec handler1 (layers) = ()




        ()

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
            
            Ok ()
        | false ->
            
            failwith "Non strict mode to be implemented."
        
        






        ()


    ()
