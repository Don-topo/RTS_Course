public interface ICommand
{
    public bool IsSingleUnitCommand { get; }
    bool CanHandle(CommandContext commandContext);
    void Handle(CommandContext commandContext);
}
