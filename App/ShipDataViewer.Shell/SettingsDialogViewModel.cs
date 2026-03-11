using ShipDataViewer.Core.Core;

using System.Collections.ObjectModel;

namespace ShipDataViewer.Shell;

public class SettingsDialogViewModel : ViewModel
{
	private readonly ServiceLoader _serviceLoader;

	public bool Saved { get; set; }

	public string? ApiKey { get; set; }

	public ObservableCollection<ServiceLoadContext>? ServiceContexts { get; set; }

	public ServiceLoadContext? SelectedServiceContext;

	public SettingsDialogViewModel(ServiceLoader serviceLoader)
	{
		_serviceLoader = serviceLoader;
	}

	public void Initialize()
	{
		ServiceContexts = new ObservableCollection<ServiceLoadContext>(_serviceLoader.ServiceContexts.Value);
	}

	public void Save()
	{
		Validate();

		if (HasErrors)
		{
			return;
		}

		Saved = true;
		OnClose();
	}

	public void Cancel()
	{
		Saved = false;
		OnClose();
	}

	protected override List<Rule> SetupValidationRules()
	{
		var rules = base.SetupValidationRules();

		rules.Add(new Rule()
			.ForProperty(nameof(ApiKey))
			.IsInvalidIf(() => string.IsNullOrWhiteSpace(ApiKey) || ApiKey?.Length != 40)
			.Message("The API key must be 40 chars in length!"));

		return rules;
	}
}
