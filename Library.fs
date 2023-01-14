module FsExecute

open System.Diagnostics
open System.Threading.Tasks

type ExitStatus =
    | Success
    | Failure

type CommandResult =
    { ExitCode: int
      StandardOutput: string
      StandardError: string }

let ExecuteCommand: string -> seq<string> -> Async<CommandResult> =
    fun executable args ->
        async {
            let startInfo = ProcessStartInfo()
            startInfo.FileName <- executable

            for a in args do
                startInfo.ArgumentList.Add(a)

            startInfo.RedirectStandardOutput <- true
            startInfo.RedirectStandardError <- true
            startInfo.UseShellExecute <- false
            startInfo.CreateNoWindow <- true

            use p = new Process()
            p.StartInfo <- startInfo
            p.Start() |> ignore

            let outTask =
                Task.WhenAll([| p.StandardOutput.ReadToEndAsync(); p.StandardError.ReadToEndAsync() |])

            do! p.WaitForExitAsync() |> Async.AwaitTask
            let! out = outTask |> Async.AwaitTask

            return
                { ExitCode = p.ExitCode
                  StandardOutput = out.[0]
                  StandardError = out.[1] }
        }

let Bind: CommandResult -> (ExitStatus * (CommandResult -> unit)) -> CommandResult =
    fun result (status, processer) ->
        (match status with
         | Success ->
             if result.ExitCode = 0 then
                 processer result
         | Failure ->
             if result.ExitCode <> 0 then
                 processer result)

        result

let (<|>) = Bind

let Exec: string -> Async<CommandResult> =
    fun command -> ExecuteCommand "/usr/bin/env" [ "-S"; "bash"; "-c"; command ]

let ExecAsync: string -> CommandResult =
    fun command -> Exec command |> Async.RunSynchronously
