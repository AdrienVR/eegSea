using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ConfigUI : MonoBehaviour
{

	public Group group;
	public List<Group> list;
	public SkullMenu skm;

	public void init(Group g, List<Group> l, SkullMenu s)
	{
		setGroup (g);
		setList (l);
		setSkull (s);
	}

	public void setList(List<Group> l)
	{
		list = l;
	}

	public void setGroup(Group g)
	{
		group = g;
		setText (g.getText());
	}

	public void setText(string s)
	{
		Text t = gameObject.GetComponentInChildren<Text> ();
		t.text = s;
	}

	public void setSkull(SkullMenu s)
	{
		skm = s;
	}

	public void erase()
	{
		list.Remove (group);
		skm.testValid ();
		skm.reDrawConfig ();
		Destroy (gameObject);
	}
	                  
}
