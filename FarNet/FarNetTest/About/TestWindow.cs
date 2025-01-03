using FarNet;

namespace FarNetTest.About;

public class TestWindow : IWindow
{
	public override int Count => throw new NotImplementedException();

	public override WindowKind Kind => WindowKind.None;

	public override bool IsModal => throw new NotImplementedException();

	public override nint GetIdAt(int index)
	{
		throw new NotImplementedException();
	}

	public override WindowKind GetKindAt(int index)
	{
		throw new NotImplementedException();
	}

	public override string GetNameAt(int index)
	{
		throw new NotImplementedException();
	}

	public override void SetCurrentAt(int index)
	{
		throw new NotImplementedException();
	}
}
