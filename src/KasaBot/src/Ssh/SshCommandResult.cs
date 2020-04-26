namespace KasaBot.Ssh
{
    public sealed class SshCommandResult
    {
        public int ExitCode { get; }
        public string StandardOutput { get; }
        public string ErrorOutput { get; }

        public SshCommandResult(int exitCode, string standardOutput, string errorOutput)
        {
            ExitCode = exitCode;
            StandardOutput = standardOutput;
            ErrorOutput = errorOutput;
        }

        public override string ToString()
        {
            return $"Code {ExitCode}: Error={ErrorOutput} | Output={StandardOutput}";
        }
    }
}