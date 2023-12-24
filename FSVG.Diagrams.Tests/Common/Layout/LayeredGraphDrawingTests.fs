namespace FSVG.Diagrams.Tests.Common.Layout

open System
open FSVG.Diagrams.Common.Layout
open FSVG.Diagrams.Common.Shared
open Microsoft.VisualStudio.TestTools.UnitTesting

// No warning needed for use of DiagramNode and Diagram, internal usage.
#nowarn "10001"

[<TestClass>]
type LayeredGraphDrawingTests() =

    [<TestMethod>]
    member _.``createLayers basic test``() =

        let nodeA =
            ({ Id = "node_a"
               Connections =
                 [ { ToId = "node_b"
                     TwoWay = false
                     Class = None } ]
               Class = None }
            : Definitions.DiagramNode)

        let nodeB =
            ({ Id = "node_b"
               Connections =
                 [ { ToId = "node_c"
                     TwoWay = false
                     Class = None } ]
               Class = None }
            : Definitions.DiagramNode)

        let nodeC =
            ({ Id = "node_c"
               Connections =
                 [ { ToId = "node_a"
                     TwoWay = false
                     Class = None } ]
               Class = None }
            : Definitions.DiagramNode)

        let internalNodeA =
            ({ Node = nodeA
               PreferredOrder = 0
               ConnectionsFrom = [ "node_c" ] }
            : LayeredGraphDrawing.InternalNode)

        let internalNodeB =
            ({ Node = nodeB
               PreferredOrder = 0
               ConnectionsFrom = [ "node_a" ] }
            : LayeredGraphDrawing.InternalNode)

        let internalNodeC =
            ({ Node = nodeC
               PreferredOrder = 0
               ConnectionsFrom = [ "node_b" ] }
            : LayeredGraphDrawing.InternalNode)

        let nodes: LayeredGraphDrawing.InternalNode list =
            [ internalNodeA; internalNodeB; internalNodeC ]

        let parameters =
            ({ Nodes = [ nodeA; nodeB; nodeC ]
               Settings =
                 { NormalizeNodes = true
                   NodeLimit = Some 1024
                   StringComparison = StringComparison.OrdinalIgnoreCase
                   StrictMode = true
                   PreferredOrderHandler = None } }
            : LayeredGraphDrawing.Parameters)

        let expected: LayeredGraphDrawing.NodeLayer list =
            [ { Level = 0; Nodes = [ internalNodeA ] }
              { Level = 1; Nodes = [ internalNodeB ] }
              { Level = 2; Nodes = [ internalNodeC ] } ]

        let actual = LayeredGraphDrawing.createLayers parameters nodes

        Assert.AreEqual(expected, actual)
