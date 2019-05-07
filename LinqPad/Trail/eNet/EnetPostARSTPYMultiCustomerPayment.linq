<Query Kind="Program">
  <Output>DataGrids</Output>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Xml.Linq</Namespace>
</Query>

void Main()
{
	var customers = new[] { "Tom", "Dick", "Harry" };
	var invoiceAutoPayment = new InvoiceAutoPayment(customers, "whatever");
	var xmlGenerator = new EnetPostARSTPYAutoPayment(invoiceAutoPayment);

	xmlGenerator.GetXmlInString().Dump("XmlIn");
	xmlGenerator.GetXmlParamsString().Dump("XmlParams");

}

// Define other methods and classes here
public class InvoiceAutoPayment
{
	public InvoiceAutoPayment(string[] customers, string reference = null)
	{
		Customers = customers;
		Bank = "10";
		Reference = reference ?? string.Empty;

		PostingPeriod = "C";
		IgnoreWarnings = true;
	}

	public string[] Customers { get; private set; }
	public string Bank { get; private set; }
	public string Reference { get; private set; }
	public bool ApplyAutomaticPayments { get; private set; } = true;
	public string AutomaticPaymentsSequence { get; private set; } = "D";
	public bool AutoPaymentsIncludeFutureInvoices { get; private set; } = false;
	public bool SumCreditValues { get; private set; } = true;
	public bool SumCreditValuesIncludeFutureInvoices { get; private set; } = false;

	public string PostingPeriod { get; private set; }
	public bool IgnoreWarnings { get; private set; }
}


abstract class EnetXmlGenerator
{
	internal static readonly string ParametersNodeName = "Parameters";

	public EnetXmlGenerator(bool validateOnly = false, bool ignoreWarnings = true)
	{
		ParamValidateOnly = validateOnly;
		ParamIgnoreWarnings = ignoreWarnings;
	}

	public bool ParamValidateOnly { get; internal set; }
	public bool ParamIgnoreWarnings { get; internal set; }

	public abstract XElement GetXmlIn();
	public abstract XElement GetXmlParams();

	public string GetXmlInString() => GetXmlIn().ToString();
	public string GetXmlParamsString() => GetXmlParams().ToString();

	internal XElement GetParamsElements(string rootName, params XElement[] extraParams)
	{
		return new XElement(rootName,
			new XElement(ParametersNodeName,
				new XElement("IgnoreWarnings", ParamIgnoreWarnings.ToYesNo()),
				new XElement("ValidateOnly", ParamValidateOnly.ToYesNo()),
				extraParams));
	}
}


class EnetPostARSTPYAutoPayment : EnetXmlGenerator
{
	private readonly InvoiceAutoPayment _autoPayment;

	public EnetPostARSTPYAutoPayment(InvoiceAutoPayment autoPayment)
	{
		_autoPayment = autoPayment;
	}

	public override XElement GetXmlIn()
	{
		return new XElement("PostArPayment",
			_autoPayment.Customers.Select(customer =>
				new XElement("Item",
					new XElement("Payment",
						new XElement("Customer", customer),
						new XElement("Reference", _autoPayment.Reference),
						new XElement("Bank", _autoPayment.Bank),
						new XElement("AutomaticPaymentsOptions",
							new XElement("ApplyAutomaticPayments", _autoPayment.ApplyAutomaticPayments.ToYesNo()),
							new XElement("AutomaticPaymentsSequence", _autoPayment.AutomaticPaymentsSequence),
							new XElement("IncludeFutureInvoices", _autoPayment.AutoPaymentsIncludeFutureInvoices.ToYesNo())
						),
						new XElement("SumCreditValuesOptions",
							new XElement("SumCreditValues", _autoPayment.SumCreditValues.ToYesNo()),
							new XElement("IncludeFutureInvoices", _autoPayment.SumCreditValuesIncludeFutureInvoices.ToYesNo())
						)
					)
				)
			)
		);
	}

	public override XElement GetXmlParams()
	{
		return GetParamsElements("PostArPayment",
			new XElement("PostingPeriod", _autoPayment.PostingPeriod),
			new XElement("Foo", DateTime.Now.ToShortDateString()));
	}
}

static class SerializationExtensions
{
	public static string ToYesNo(this bool predicate) => predicate ? "Y" : "N";
}