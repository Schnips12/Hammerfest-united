using System.Collections.Generic;

///</Summary>A DepthManager handles the depth sorting of MovieClips.
/// Each SortingLayer is managed as a group (plan) of up to 1000 elements, all of different Z depth.
/// A DepthManager can reference one group per SortingLayer. A default layer is used when not explicitly instructed otherwise.
/// When using several DepthManagers at once, if they use the same SortingLayers, conflicts might arise.</summary>
public class DepthManager
{    
    public IMovieClip hierarchyParent;
	string defaultLayer;

    class Plan {
		public string name;
        public List<IMovieClip> tbl;
        public int cur;
        public Plan(string name) {
			this.name = name;
            tbl = new List<IMovieClip>();
            cur = 0;
        }
    }
	List<Plan> plans;

	public DepthManager(IMovieClip mc, string layer) {
		hierarchyParent = mc;
		defaultLayer = layer;
		plans = new List<Plan>();
	}

	public void SetName(string name) {
		hierarchyParent._name = name;
	}

	public void SetRoot(MovieClip newRoot) {
		hierarchyParent = newRoot;
	}

	private Plan GetPlan(string layer) {
		Plan plan_data = plans.Find(x => x.name == layer);
		if (plan_data==null) {
			plan_data = new Plan(layer);
			plans.Add(plan_data);
		}

		return plan_data;
	}

	void Compact(Plan plan_data) {
		List<IMovieClip> p = plan_data.tbl;
		int max = plan_data.cur;
		int i;
		int cur = 0;
		for(i=0 ; i<max ; i++)
			if( p[i].united != null ) {
				p[i].SetDepth(cur);
				p[cur] = p[i];
				cur++;
			}
		plan_data.cur = cur;
	}

	public IMovieClip Attach(IMovieClip mc)
	{
		return Attach(mc, defaultLayer);
	}

	public IMovieClip Attach(IMovieClip mc, string layer) {
		Plan plan_data = GetPlan(layer);
		List<IMovieClip> p = plan_data.tbl;
		int d = plan_data.cur;
		if(d == 1000) {
			Compact(plan_data);
			return Attach(mc, layer);
		}
		mc.SetParent(hierarchyParent);
		mc.SetLayer(layer);
		mc.SetDepth(d);
		p.Add(mc);
		plan_data.cur++;
		return mc;
	}

	public MovieClip Empty()
	{
		return Empty(defaultLayer);
	}

	public MovieClip Empty(string layer) {
		Plan plan_data = GetPlan(layer);
		List<IMovieClip> p = plan_data.tbl;
		int d = plan_data.cur;
		if(d == 1000) {
			Compact(plan_data);
			return Empty();
		}
		MovieClip mc = new MovieClip("Empty");
		mc.SetParent(hierarchyParent);
		mc.SetLayer(layer);
		mc.SetDepth(d);
		p.Add(mc);
		plan_data.cur++;
		return mc;
	}

	int Reserve(IMovieClip mc, string layer) {
		Plan plan_data = GetPlan(layer);
		List<IMovieClip> p = plan_data.tbl;
		int d = plan_data.cur;
		if(d == 1000) {
			Compact(plan_data);
			return Reserve(mc, layer);
		}
		p[d] = mc;
		plan_data.cur++;
		return d;
	}

	public void Swap(IMovieClip mc, string layer) {
		string src_plan = mc.GetLayer();
		if( src_plan == layer )
			return;
		Plan plan_data = GetPlan(src_plan);
		List<IMovieClip> p = plan_data.tbl;
		int max = plan_data.cur;
		int i;
		for(i=0 ; i<max ; i++)
			if( p[i] == mc ) {
				p[i] = null;
				break;
			}
		mc.SetLayer(layer);
		mc.SetDepth(Reserve(mc, layer));
	}

	void Under(IMovieClip mc) {
		int d = mc.GetDepth();
		string layer = mc.GetLayer();
		Plan plan_data = GetPlan(layer);
		List<IMovieClip> p = plan_data.tbl;
		if(p[d] == mc) {
			p.RemoveAt(d);
			p.Insert(0, mc);
		}
	}

	void Over(IMovieClip mc) {
		int d = mc.GetDepth();
		string layer = mc.GetLayer();
		Plan plan_data = GetPlan(layer);
		List<IMovieClip> p = plan_data.tbl;
		if( p[d] == mc ) {
			p.RemoveAt(d);
			p.Add(mc);
		}
	}

	void Clear(Plan plan_data) {
		int i;
		List<IMovieClip> p = plan_data.tbl;
		for(i=0 ; i<plan_data.cur ; i++) {
			p[0].RemoveMovieClip();
			p.RemoveAt(0);
		}
		plan_data.cur = 0;
	}

	void Ysort(string layer) {
		Plan plan_data = GetPlan(layer);
		List<IMovieClip> p = plan_data.tbl;
		int len = plan_data.cur;
		int i, j;
		float y = -99999999;
		for(i=0 ; i<len ; i++) {
			IMovieClip mc = p[i];
			float mcy = mc._y;
			if( mcy >= y )
				y = mcy;
			else {
				for(j=i;j>0;j--) {
					IMovieClip mc2 = p[j-1];
					if( mc2._y > mcy ) {
						p[j] = mc2;
						mc.SwapDepths(mc2);
					} else {
						p[j] = mc;
						break;
					}
				}
				if( j == 0 )
					p[0] = mc;
			}
		}
	}

	public void DestroyThis() {
		foreach (Plan p in plans)
		{
			Clear(p);
		}
	}
}
