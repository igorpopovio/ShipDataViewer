namespace ShipDataViewer.Core.Core;

public class Rule
{
	public string PropertyName { get; private set; } = string.Empty;
	public Func<bool>? InvalidIf { get; private set; }
	public string ErrorMessage { get; private set; } = string.Empty;
	public bool IsValid { get; private set; }

	public Rule ForProperty(string propertyName)
	{
		PropertyName = propertyName;
		return this;
	}

	public Rule IsInvalidIf(Func<bool> invalidIf)
	{
		InvalidIf = invalidIf;
		return this;
	}

	public Rule Message(string errorMessage)
	{
		ErrorMessage = errorMessage;
		return this;
	}

	public bool Validate()
	{
		IsValid = !(InvalidIf?.Invoke() ?? false);
		return IsValid;
	}
}
