using MessagePipe;
using System;

public static class TestHelper
{
    public static IServiceProvider BuildBuiltin(Action<MessagePipeOptions> configure, Action<BuiltinContainerBuilder> use)
    {
        var builder = new BuiltinContainerBuilder();
        builder.AddMessagePipe(configure);
        use(builder);

        return builder.BuildServiceProvider();
    }

    public static IServiceProvider BuildBuiltin(Action<BuiltinContainerBuilder> use)
    {
        var builder = new BuiltinContainerBuilder();
        builder.AddMessagePipe();
        use(builder);

        return builder.BuildServiceProvider();
    }
}
