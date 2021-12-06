using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalLink {

    // from
	int from_did;
	int from_lid;
	int from_pid;

	// to
	int to_did;
	int to_lid;
	int to_pid;

	PortalLink() {
	}

	public void CleanUp() {
		// Do nothing
	}

	public void Trace() {
		Debug.Log("link: "+from_did+","+from_lid+"("+from_pid+")  > "+to_did+","+to_lid+"("+to_pid+")");
	}
}