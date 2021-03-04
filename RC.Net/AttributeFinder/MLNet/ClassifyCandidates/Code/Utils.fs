module Extract.AttributeFinder.MLNet.ClassifyCandidates.Utils

open System.Text
open System.Security.Cryptography
open System.Threading

let hashString (text: string) = 
  text
  |> Encoding.UTF8.GetBytes
  |> (new SHA256Managed()).ComputeHash
  |> Array.map (fun b -> b.ToString("x2"))
  |> String.concat ""
(************************************************************************************************************************)

// Handle an abandoned mutex gracefully
// in case a process is killed at the wrong time or something
let acquireMutex (millisecondsTimeout: int) (mutex: Mutex) =
  try mutex.WaitOne millisecondsTimeout with | :? AbandonedMutexException -> true
(************************************************************************************************************************)

