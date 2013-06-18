using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web.UI;
using System.Web.UI.Adapters;

namespace DevelopmentWithADot.AspNetOutputAdapterControl
{
	[NonVisualControl]
	public class OutputAdapterControl : Control
	{
		private static readonly FieldInfo occasionalFieldsField = typeof(Control).GetField("_occasionalFields", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo flagsField = typeof(Control).GetField("flags", BindingFlags.NonPublic | BindingFlags.Instance);

		public OutputAdapterControl()
		{
			this.Enabled = true;
		}

		public String XslPath
		{
			get;
			set;
		}

		public String TargetControlID
		{
			get;
			set;
		}

		public Boolean Enabled
		{
			get;
			set;
		}

		public event EventHandler<OutputEventArgs> Output;

		private ControlAdapter getControlAdapter(Control control)
		{
			Object flags = flagsField.GetValue(control);
			MethodInfo setMethod = flags.GetType().GetMethod("Set", BindingFlags.NonPublic | BindingFlags.Instance);
			setMethod.Invoke(flags, new Object[] { 0x8000 });

			Object occasionalFields = occasionalFieldsField.GetValue(control);
			FieldInfo rareFieldsField = occasionalFields.GetType().GetField("RareFields");
			Object rareFields = rareFieldsField.GetValue(occasionalFields);

			if (rareFields == null)
			{
				rareFields = FormatterServices.GetUninitializedObject(rareFieldsField.FieldType);
				rareFieldsField.SetValue(occasionalFields, rareFields);
			}

			FieldInfo adapterField = rareFields.GetType().GetField("Adapter");
			ControlAdapter adapter = adapterField.GetValue(rareFields) as ControlAdapter;

			return (adapter);
		}

		private void setControlAdapter(Control control, ControlAdapter controlAdapter)
		{
			Object occasionalFields = occasionalFieldsField.GetValue(control);
			FieldInfo rareFieldsField = occasionalFields.GetType().GetField("RareFields");
			Object rareFields = rareFieldsField.GetValue(occasionalFields);
			FieldInfo adapterField = rareFields.GetType().GetField("Adapter");
			adapterField.SetValue(rareFields, controlAdapter);
		}

		internal void RaiseOutputEvent(OutputEventArgs e)
		{
			if (this.Output != null)
			{
				this.Output(this, e);
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			if ((this.Enabled == true) && (String.IsNullOrWhiteSpace(this.TargetControlID) == false))
			{
				Control control = this.FindControl(this.TargetControlID);
				ControlAdapter controlAdapter = this.getControlAdapter(control);
				OutputAdapterControlAdapter newAdapter = new OutputAdapterControlAdapter(this, controlAdapter, control, this.XslPath);

				this.setControlAdapter(control, newAdapter);
			}

			base.OnPreRender(e);
		}
	}
}