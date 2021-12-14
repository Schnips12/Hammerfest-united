using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Level;

public class PortalLink {

    // from
	public int from_did;
	public int from_lid;
	public int from_pid;

	// to
	public int to_did;
	public int to_lid;
	public int to_pid;

	public PortalLink() {
	}

	public void CleanUp() {
		// Do nothing
	}

	public void Trace() {
		Debug.Log("link: "+from_did+","+from_lid+"("+from_pid+")  > "+to_did+","+to_lid+"("+to_pid+")");
	}
}