namespace XJector;

public interface IInstruction
{
    public void Write(CodeWriter codeWriter);
}

public readonly struct OneLineInstruction(string instruction) : IInstruction
{
    public void Write(CodeWriter codeWriter)
    {
        string sanitizedInstruction = instruction.Trim();
        if(!sanitizedInstruction.EndsWith(";"))
        {
            sanitizedInstruction += ';';
        }
        
        codeWriter.WriteLine(sanitizedInstruction);
    }
}

public readonly struct IfInstruction(string condition, IInstruction[] instructions) : IInstruction
{
    public void Write(CodeWriter codeWriter)
    {
        codeWriter.WriteLine($"if({condition})");
        codeWriter.InBlock(instructions);
    }
}