namespace FSVG.Diagrams.Common.Rendering

// No warning needed for use of DiagramNode and Diagram
#nowarn "10001"

[<AutoOpen>]
module Common =

    open FSVG.Diagrams.Common.Shared

    type RenderingType = | Grid

    and GridRenderingSettings =
        { Rows: GridRow list }

        member grs.RowCount() = grs.Rows.Length

        member grs.ColumnCount() =
            grs.Rows |> List.maxBy (fun r -> r.ColumnCount()) |> (fun r -> r.ColumnCount())

    and GridRow =

        { Order: int
          Nodes: GridNode list }

        member gr.ColumnCount() = gr.Nodes |> List.maxBy (fun r -> r.Column) |> fun r -> r.Column

    and GridNode =
        { Node: Definitions.DiagramNode
          Column: int }


    ()
