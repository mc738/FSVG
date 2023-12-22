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
          ConnectionsFrom: string list }

    let createInternalNodes (parameters: Parameters) =
        // PERFORMANCE Could this be optimized?
        parameters.Nodes
        |> List.map (fun n ->

            { Node = n
              ConnectionsFrom =
                parameters.Nodes
                |> List.filter (fun on ->
                    on.Connections
                    |> List.exists (fun nc -> nc.ToId.Equals(n.Id, parameters.Settings.StringComparison)))
                |> List.map (fun on -> on.Id) })
    
    type VerificationResultItem = { FromNode: string; ToNode: string }
    
    type VerificationResult =
        | Success of VerificationResultItem
        | CyclicReference of VerificationResultItem
        | NodeNotFound of VerificationResultItem

    type VerificationResults =
        { Successes: VerificationResultItem list
          CyclicReferences: VerificationResultItem list
          NodesNotFound: VerificationResultItem list }
        
        //static member Create(results: VerificationResult list) =
            

    let contains (comparison: StringComparison) (value: string) (values: string list) =
        values |> List.exists (fun v -> v.Equals(value, comparison))

    let verifyInternalNodes (parameters: Parameters) (nodes: InternalNode list) =
        // For go FP for the sake of internal usage.
        // This are oversize, but in practice that shouldn't matter,
        // this is just a optimization to make the verification process a bit quicker/less resource intensive.
        // This uses nodes.Length / 2 for the second 2 because in general there should be less of these.
        
        let successes = ResizeArray<VerificationResultItem>(nodes.Length)
        let cyclicReferences = ResizeArray<VerificationResultItem>(nodes.Length / 2)
        let nodesNotFound = ResizeArray<VerificationResultItem>(nodes.Length /  2)
        
        //    results |> List.iter (fun r ->
        //        match r with
        //        | Success vri -> s.Add vri
        //        | CyclicReference vri -> cr.Add vri
        //        | NodeNotFound vri -> nnf.Add vri)
            
            
        
        
        let nodesMap = nodes |> List.map (fun n -> n.Node.Id, n) |> Map.ofList

        nodes
        |> List.collect (fun n ->
            n.ConnectionsFrom
            |> List.map (fun ncf ->
                match nodesMap.TryFind ncf with
                | Some fn ->
                    match fn.ConnectionsFrom |> contains parameters.Settings.StringComparison n.Node.Id with
                    | true ->
                        { FromNode = fn.Node.Id
                          ToNode = n.Node.Id }
                        |> cyclicReferences.Add
                        
                        
                        VerificationResult.CyclicReference(n.Node.Id, fn.Node.Id)
                    | false -> VerificationResult.Success(n.Node.Id, fn.Node.Id)
                | None -> VerificationResult.NodeNotFound(n.Node.Id, ncf)))



    let createLayers (parameters: Parameters) =
        let rec handler1 (layers) = ()




        ()

    let handle (parameters: Parameters) =

        // Pre - create and verify internal nodes.
        let internalNodes = createInternalNodes parameters

        let verificationResults = verifyInternalNodes parameters internalNodes



        // First create a directed acyclic graph
        // This also needs to verify data to make sure cycles don't exist.
        // Where the they are used `TwoWay` should be used instead.

        // Next create layers, the goal is to minimise the number of layers
        // and limit the number of that span multiple layers

        // Then we can start working out placement within layers and connections






        ()


    ()
