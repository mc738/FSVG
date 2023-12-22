namespace FSVG.Diagrams.Common.Layout

// No warning needed for use of DiagramNode and Diagram, internal usage.
#nowarn "10001"

open FSVG.Diagrams.Common.Shared

[<RequireQualifiedAccess>]
module LayeredGraphDrawing =

    open FSVG.Diagrams.Common.Shared

    // Implementation based on https://en.wikipedia.org/wiki/Layered_graph_drawing

    // https://www.baeldung.com/cs/graph-auto-layout-algorithm

    type Settings = {
        NormalizeNodes: bool
        NodeLimit: int option
    }
    
    type Parameters =
        {
            Nodes: Definitions.DiagramNode list
            Settings: Settings
        }
        
    type Layer =
        {
            Nodes: Definitions.DiagramNode
            Order: int
        }

    
    let createLayers (parameters: Parameters) =
        let rec handler1 () =
            ()
        
        ()
     
    let handle (parameters: Parameters) =

        // First create a directed acyclic graph
        // This also needs to verify data to make sure cycles don't exist.
        // Where the they are used `TwoWay` should be used instead.

        // Next create layers, the goal is to minimise the number of layers
        // and limit the number of that span multiple layers
        
        // Then we can start working out placement within layers and connections
        
        
        
        
        
        
        ()


    ()
