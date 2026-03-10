using ShipDataViewer.Core.Core;

using System.Collections;
using System.ComponentModel;

namespace ShipDataViewer.Core.Model;

public abstract partial class Model : INotifyPropertyChanged, INotifyDataErrorInfo
{
	private List<Rule> _rules;

	public Model()
	{
		_rules = SetupValidationRules();
	}

	protected virtual List<Rule> SetupValidationRules() => [];

	protected virtual void Validate()
	{
		foreach (var rule in _rules)
		{
			rule.Validate();
			if (!rule.IsValid)
			{
				HasErrors = true;
				OnErrorsChanged(rule.PropertyName);
				return;
			}
		}

		HasErrors = false;
	}

	public bool HasErrors { get; private set; }

	public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

	protected virtual void OnErrorsChanged(string propertyName)
	{
		ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
	}

	public IEnumerable GetErrors(string? propertyName)
	{
		return _rules
			.Where(r => r.PropertyName == propertyName && !r.IsValid)
			.Select(r => r.ErrorMessage);
	}
}
