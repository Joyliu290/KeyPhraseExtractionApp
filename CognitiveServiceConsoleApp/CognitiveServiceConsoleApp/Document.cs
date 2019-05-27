using System;

public class Document
{
    private string language;
    private string id;
    private string text;

    public Document()
	{
	}

    public string Language{ get { return language; } set { language = value; }}
    public string Id { get { return id; } set { id = value; } }
    public string Text { get { return text; } set { text = value; } }
}
