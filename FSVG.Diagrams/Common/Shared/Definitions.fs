namespace FSVG.Diagrams.Common.Shared

// No warning needed for use of DiagramNode and Diagram
#nowarn "10001"

[<RequireQualifiedAccess>]
module Definitions =

    [<Literal>]
    let private typeUsageWarningMessage =
        "This type is intended for internal use only. It is recommended to create a higher level type for a specific diagram and map it back to this type."

    /// <summary>
    /// A node on a diagram. This is intended
    /// </summary>
    [<CompilerMessage(typeUsageWarningMessage, 10001)>]
    type DiagramNode =
        { Id: string
          Class: string option
          Connections: DiagramNodeConnection list }

    and [<CompilerMessage(typeUsageWarningMessage, 10001)>] DiagramNodeConnection =
        {
            ToId: string
            TwoWay: bool
            /// <summary>
            /// The class is a default optional raw string value.
            /// When it comes to rendering it is up to the diagram rendered to handle this.
            /// </summary>
            Class: string option
        }

        static member Create(toId: string, ?twoWay: bool, ?className: string) =
            { ToId = toId
              TwoWay = twoWay |> Option.defaultValue false
              Class = className }
