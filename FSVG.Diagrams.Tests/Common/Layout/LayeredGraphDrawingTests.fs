namespace FSVG.Diagrams.Tests.Common.Layout

open System
open FSVG.Diagrams.Common.Layout
open Microsoft.VisualStudio.TestTools.UnitTesting

[<TestClass>]
type LayeredGraphDrawingTests() =

    [<TestMethod>]
    member _.``createLayers basic test``() =

        let nodes: LayeredGraphDrawing.InternalNode list =
            [ { Node =
                  { Id = "node_a"
                    Connections =
                      [ { ToId = "node_b"
                          TwoWay = false
                          Class = None } ]
                    Class = None }
                PreferredOrder = 0
                ConnectionsFrom = [ "node_c" ] }
              { Node =
                  { Id = "node_b"
                    Connections =
                      [ { ToId = "node_c"
                          TwoWay = false
                          Class = None } ]
                    Class = None }
                PreferredOrder = 0
                ConnectionsFrom = [ "node_a" ] }
              { Node =
                  { Id = "node_c"
                    Connections =
                      [ { ToId = "node_a"
                          TwoWay = false
                          Class = None } ]
                    Class = None }
                PreferredOrder = 0
                ConnectionsFrom = [ "node_b" ] } ]

        let parameters =
            ({ Nodes = []
               Settings =
                 { NormalizeNodes = true
                   NodeLimit = Some 1024
                   StringComparison = StringComparison.OrdinalIgnoreCase
                   StrictMode = true
                   PreferredOrderHandler = None } }
            : LayeredGraphDrawing.Parameters)

        let actual = LayeredGraphDrawing.createLayers

        ()
