module FsExecute

type ExitStatus =
    | Success
    | Failure

type CommandResult =
    { ExitCode: int
      StandardOutput: string
      StandardError: string }


val Exec: string -> Async<CommandResult>
val ExecAsync: string -> CommandResult

val Bind: CommandResult -> (ExitStatus * (CommandResult -> unit)) -> CommandResult
val (<|>): (CommandResult -> (ExitStatus * (CommandResult -> unit)) -> CommandResult)
