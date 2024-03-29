﻿namespace FSVG.Diagrams.Common.Shared

// No warning needed for use of DiagramNode and Diagram
#nowarn "10001"

open System

module Rendering =

    open FSVG.Diagrams.Common.Shared

    [<RequireQualifiedAccess>]
    type RenderingMode = | Grid

    type RendererSettings = Grid of GridRendererSettings

    and RenderingUnit =
        | Fixed of float
        | Percentage of float
        | Dynamic

    and GridRendererSettings =
        { Rows: GridRow list
          Properties: Map<string, string>
          Classes: string list }

        member grs.TryGetProperty(key: string) = grs.Properties.TryFind key

        member grs.TryGetPropertyAsFloat(key: string) =
            grs.TryGetProperty key
            |> Option.bind (fun p ->
                match Double.TryParse p with
                | true, v -> Some v
                | false, _ -> None)

        member grs.RowCount() = grs.Rows.Length

        member grs.ColumnCount() =
            grs.Rows |> List.maxBy (fun r -> r.ColumnCount()) |> (fun r -> r.ColumnCount())

    and GridRow =

        { Order: int
          Height: RenderingUnit
          Nodes: GridNode list }

        member gr.ColumnCount() = gr.Nodes.Length

    and GridNode =
        { Node: Definitions.DiagramNode
          Width: RenderingUnit
          Column: int }

    let run (settings: RendererSettings) =
        match settings with
        | Grid gridRendererSettings ->

            // TODO make configurable, add margins etc.
            let padding = gridRendererSettings.TryGetPropertyAsFloat "padding" |> Option.defaultValue 5.
            
            
            
            
            gridRendererSettings.Rows

