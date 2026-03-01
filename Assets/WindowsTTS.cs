using UnityEngine;
using System.Diagnostics;

public class WindowsTTS : MonoBehaviour
{
    private Process currentProcess;

    public void Speak(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        text = text.Replace("\"", "'").Replace("\n", " ");

        string command =
            "Add-Type -AssemblyName System.Speech; " +
            "$speak = New-Object System.Speech.Synthesis.SpeechSynthesizer; " +
            "$speak.Rate = 2; " +
            "$speak.Speak('" + text + "');";

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe",
            Arguments = "-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -Command \"" + command + "\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = false,
            RedirectStandardError = false
        };

        // Cerrar proceso anterior si aún sigue activo
        if (currentProcess != null && !currentProcess.HasExited)
        {
            currentProcess.Kill();
        }

        currentProcess = Process.Start(psi);
    }
}