using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.Adapters;
using System.Xml;
using System.Xml.Xsl;

namespace DevelopmentWithADot.AspNetOutputAdapterControl
{
	public class OutputAdapterControlAdapter : ControlAdapter
	{
		private static readonly FieldInfo controlField = typeof(ControlAdapter).GetField("_control", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo controlRenderMethod = typeof(Control).GetMethod("Render", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo controlAdapterRenderMethod = typeof(ControlAdapter).GetMethod("Render", BindingFlags.NonPublic | BindingFlags.Instance);

		public OutputAdapterControlAdapter(OutputAdapterControl outputControl, ControlAdapter original, Control control, String xslPath)
		{
			this.OutputControl = outputControl;
			this.Original = original;
			this.XslPath = xslPath;
			controlField.SetValue(this, control);
		}

		protected OutputAdapterControl OutputControl
		{
			get;
			private set;
		}

		protected String XslPath
		{
			get;
			private set;
		}

		protected ControlAdapter Original
		{
			get;
			private set;
		}

		protected override void Render(HtmlTextWriter writer)
		{
			StringBuilder builder = new StringBuilder();
			HtmlTextWriter tempWriter = new HtmlTextWriter(new StringWriter(builder));

			if (this.Original != null)
			{
				controlAdapterRenderMethod.Invoke(this.Original, new Object[] { tempWriter });
			}
			else
			{
				controlRenderMethod.Invoke(this.Control, new Object[] { tempWriter });
			}

			if (String.IsNullOrWhiteSpace(this.XslPath) == false)
			{
				String path = HttpContext.Current.Server.MapPath(this.XslPath);

				XmlDocument xml = new XmlDocument();
				xml.LoadXml(builder.ToString());

				builder.Clear();

				XslCompiledTransform xsl = new XslCompiledTransform();
				xsl.Load(path);
				xsl.Transform(xml, null, tempWriter);
			}

			OutputEventArgs e = new OutputEventArgs() { Html = builder.ToString() };

			this.OutputControl.RaiseOutputEvent(e);

			if (e.Html != builder.ToString())
			{
				builder.Clear();
				builder.Append(e.Html);
			}

			writer.Write(builder.ToString());
		}
	}
}