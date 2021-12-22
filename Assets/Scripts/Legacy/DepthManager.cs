using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthManager
{    
    MovieClip root_mc;

    class Plan {
        public List<MovieClip> tbl;
        public int cur;
        public Plan() {
            tbl = new List<MovieClip>();
            cur = 0;
        }
    }
	List<Plan> plans;

	public DepthManager(MovieClip mc) {
		root_mc = mc;
		plans = new List<Plan>();
	}

	private Plan GetPlan(int pnb) {
		while(plans.Count <= pnb) {
			plans.Add(new Plan());
		}
		Plan plan_data = plans[pnb];
		if(plan_data == null) {
			plan_data = new Plan();
			plans[pnb] = plan_data;
		}
		return plan_data;
	}

	void Compact(int plan) {
		var plan_data = plans[plan];
		var p = plan_data.tbl;
		var max = plan_data.cur;
		int i;
		int cur = 0;
		int b = plan * 1000;
		for(i=0 ; i<max ; i++)
			if( p[i]._name != null ) {
				p[i].SwapDepths(b+cur);
				p[cur] = p[i];
				cur++;
			}
		plan_data.cur = cur;
	}

	public MovieClip Attach(string inst, int plan) {
		var plan_data = GetPlan(plan);
		var p = plan_data.tbl;
		var d = plan_data.cur;
		if(d == 1000) {
			Compact(plan);
			return Attach(inst, plan);
		}
		MovieClip mc = new MovieClip(root_mc, inst, plan+d/1000);
		while(p.Count <= d) {
			p.Add(null);
		}
		p[d] = mc;
		plan_data.cur++;
		return mc;
	}

	public MovieClip Empty(int plan) {
		var plan_data = GetPlan(plan);
		var p = plan_data.tbl;
		var d = plan_data.cur;
		if(d == 1000) {
			Compact(plan);
			return Empty(plan);
		}
		MovieClip mc = new MovieClip(root_mc, plan+d/1000);
		p[d] = mc;
		plan_data.cur++;
		return mc;
	}

	int Reserve(MovieClip mc, int plan) {
		var plan_data = GetPlan(plan);
		var p = plan_data.tbl;
		var d = plan_data.cur;
		if(d == 1000) {
			Compact(plan);
			return Reserve(mc, plan);
		}
		p[d] = mc;
		plan_data.cur++;
		return d + plan * 1000;
	}

	public void Swap(MovieClip mc, int plan) {
		var src_plan = Mathf.FloorToInt(mc.GetDepth());
		if( src_plan == plan )
			return;
		var plan_data = GetPlan(src_plan);
		var p = plan_data.tbl;
		var max = plan_data.cur;
		int i;
		for(i=0 ; i<max ; i++)
			if( p[i] == mc ) {
				p[i] = null;
				break;
			}
		mc.SwapDepths(Reserve(mc, plan));
	}

	void Under(MovieClip mc) {
		var d = mc.GetDepth();
		var plan = Mathf.FloorToInt(d);
		var plan_data = GetPlan(plan);
		var p = plan_data.tbl;
		var pd = (d * 1000) % 1000;
		if(p[pd] == mc) {
			p[pd] = null;
			p.Insert(0, mc);
			plan_data.cur++;
			Compact(plan);
		}
	}

	void Over(MovieClip mc) {
		var d = mc.GetDepth();
		var plan = Mathf.FloorToInt(d);
		var plan_data = GetPlan(plan);
		var p = plan_data.tbl;
		var pd = (d * 1000) % 1000;
		if( p[pd] == mc ) {
			p[pd] = null;
			if(plan_data.cur == 1000)
				Compact(plan);
			d = plan_data.cur;
			plan_data.cur++;
			mc.SwapDepths(plan + d/1000);
			p[d] = mc;
		}
	}

	void Clear(int plan) {
		var plan_data = GetPlan(plan);
		int i;
		var p = plan_data.tbl;
		for(i=0 ; i<plan_data.cur ; i++)
			p[i].RemoveMovieClip();
		plan_data.cur = 0;
	}

	void Ysort(int plan) {
		var plan_data = GetPlan(plan);
		var p = plan_data.tbl;
		var len = plan_data.cur;
		int i, j;
		float y = -99999999;
		for(i=0 ; i<len ; i++) {
			var mc = p[i];
			var mcy = mc._y;
			if( mcy >= y )
				y = mcy;
			else {
				for(j=i;j>0;j--) {
					var mc2 = p[j-1];
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
		int i;
		for(i=0;i<plans.Count;i++)
			Clear(i);
	}
}
