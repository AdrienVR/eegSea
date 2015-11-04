using System.Collections;
using System.IO;
using System.Text;

public class XmlWriter
{

	private string path;
	private TextWriter writer;
	private int indent;

	public XmlWriter(string p)
	{
		path = p;
	}

	public void WriteStartDocument()
	{
		indent = 0;
		writer = new StreamWriter (path);
		writer.Write("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>\n");
	}

	public void WriteStartElement(string e)
	{
		for (int i=0; i<indent; i++)
			writer.Write ("\t");
		writer.Write ("<"+e+">\n");
		indent++;
	}

	public void WriteEndElement(string e)
	{
		for (int i=0; i<indent-1; i++)
			writer.Write ("\t");
		writer.Write ("</" + e + ">\n");
		indent--;
	}

	public void WriteElementString(string balise, string content)
	{
		for (int i=0; i<indent; i++)
			writer.Write ("\t");
		writer.Write ("<"+balise+">"+content+"</"+balise+">\n");
	}

	public void WriteEndDocument()
	{
		writer.Close ();
	}

}
