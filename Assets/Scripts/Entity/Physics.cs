using UnityEngine;

public class Physics : HAnimator
{
	public float? dx { get; set; }
	public float? dy { get; set; }

	protected float? fallStart; // use for fall height

	public float gravityFactor;
	public float fallFactor;
	public float? slideFriction; // default world value if "null"
	public float shockResistance; // resistance to shockwaves

	public bool fl_stable { get; set; }
	public bool fl_physics;
	public bool fl_friction;
	public bool fl_gravity;
	protected bool fl_strictGravity;
	public bool fl_hitGround;
	protected bool fl_hitCeil;
	public bool fl_hitWall;
	protected bool fl_hitBorder;
	protected bool fl_slide;
	protected bool fl_teleport;
	protected bool fl_portal;
	protected bool fl_wind;
	protected bool fl_moveable;
	protected bool fl_bump;

	protected bool fl_stopStepping;
	protected bool fl_skipNextGravity;
	protected bool fl_skipNextGround;

	public TeleporterData lastTeleporter;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public Physics(MovieClip mc) : base(mc) {

		dx = 0;
		dy = 0;
		gravityFactor = 1.0f;
		fallFactor = 1.0f;
		shockResistance = 1.0f;

		fl_stable       = false;

		fl_physics		= true;
		fl_friction		= true;
		fl_gravity		= true;
		fl_strictGravity= true;
		fl_hitGround	= true;
		fl_hitCeil		= false;
		fl_hitWall		= true;
		fl_hitBorder	= true;
		fl_slide		= true;
		fl_teleport		= false;
		fl_portal		= false;
		fl_wind			= false;
		fl_moveable		= true;
		fl_bump			= false;

		fl_stopStepping     = false;
		fl_skipNextGravity  = true;
		fl_skipNextGround   = false;
	}


	/*------------------------------------------------------------------------
	INIT
	------------------------------------------------------------------------*/
	protected override void Init(GameMode g) {
		base.Init(g);
		this.Register(Data.PHYSICS);
	}


	/*------------------------------------------------------------------------
	ACTIVATION/D�SACTIVATION DU MOTEUR PHYSIQUE
	------------------------------------------------------------------------*/
	protected void EnablePhysics() {
		fl_physics = true;
	}

	protected void DisablePhysics() {
		fl_physics = false;
	}


	/*------------------------------------------------------------------------
	EFFET D'ONDE DE CHOC AUTOUR DE L'ENTIT�
	------------------------------------------------------------------------*/
	protected void ShockWave(Physics e, float radius, float power) {
		if (!e.fl_moveable) {
			return;
		}
		power /= e.shockResistance; // le poids r�duit la puissance
		var dist = e.Distance(x, y); // Math.sqrt( Math.pow(e.x-x,2) + Math.pow(e.y-y,2) );
		var ratio = 1-dist/radius;
		var ang = Mathf.Atan2(e.y-y, e.x-x);
		e.dx = Mathf.Cos(ang) * ratio * power;
		if (e.fl_stable) {
			e.dy = -5;
		}
		else {
			e.dy += Mathf.Sin(ang) * ratio * power;
			e.dy = Mathf.Max(e.dy??0, -10);
		}
		e.fl_stable = false;
	}


	/*------------------------------------------------------------------------
	MORT AVEC ANIMATION DE SAUT
	------------------------------------------------------------------------*/
	public virtual void KillHit(float? dx) {
		if (fl_kill) {
			return;
		}

		this.dx = dx;
		this.dy = -10;
		fl_hitGround = false;
		fl_hitWall = false;
		fl_kill = true;
		fallFactor = Data.FALL_FACTOR_DEAD;
		OnKill();
	}

	/*------------------------------------------------------------------------
	R�SURRECTION
	------------------------------------------------------------------------*/
	protected virtual void Resurrect() {
		fl_hitGround = true;
		fl_hitWall = true;
		fl_kill = false;
		fallFactor = 1.0f;
		fl_stopStepping = true;
	}


	// *** MACROS DE D�PLACEMENT

	/*------------------------------------------------------------------------
	CALCULE LES DX,DY SELON UN ANGLE (EN DEGR�) ET UNE VITESSE
	------------------------------------------------------------------------*/
	public void MoveToAng(float angDeg, float speed) {
		var rad = Mathf.PI*angDeg / 180;
		dx = Mathf.Cos(rad)*speed;
		dy = Mathf.Sin(rad)*speed;
	}

	/*------------------------------------------------------------------------
	CALCULE LES DX,DY SELON UNE AUTRE ENTIT� CIBLE
	------------------------------------------------------------------------*/
	public void MoveToTarget(IEntity e, float speed) {
		var rad = Mathf.Atan2(e.y-y,e.x-x);
		dx = Mathf.Cos(rad)*speed;
		dy = Mathf.Sin(rad)*speed;
	}

	/*------------------------------------------------------------------------
	CALCULE DX / DY SELON UNE COORDONN�E
	------------------------------------------------------------------------*/
	protected void MoveToPoint(float x, float y, float speed) {
		var rad = Mathf.Atan2(y-this.y, x-this.x);
		dx = Mathf.Cos(rad)*speed;
		dy = Mathf.Sin(rad)*speed;
	}


	/*------------------------------------------------------------------------
	D�PLACEMENT DANS UNE DIRECTION AU CHOIX
	------------------------------------------------------------------------*/
	public virtual void MoveUp(float speed) {
		MoveToAng( -90, speed );
	}
	public virtual void MoveDown(float speed) {
		MoveToAng( 90, speed );
	}
	public virtual void MoveLeft(float speed) {
		MoveToAng( 180, speed );
	}
	public virtual void MoveRight(float speed) {
		MoveToAng( 0, speed );
	}


	/*------------------------------------------------------------------------
	D�PLACEMENT EN PARTANT D'UNE ENTIT�
	------------------------------------------------------------------------*/
	public void MoveFrom(Entity e, float speed) {
		x = e.x;
		y = e.y;
		var ang = -Random.Range(0, Mathf.Round(100*Mathf.PI))/100;
		x = e.x+Mathf.Cos(ang)*Data.CASE_WIDTH*2;
		y = e.y+Mathf.Sin(ang)*Data.CASE_HEIGHT*2;
		MoveToTarget(e,speed);
		dx=-dx;
		dy=-dy;
	}


	/*------------------------------------------------------------------------
	POSE L'ENTIT� AU SOL LE PLUS PROCHE (AVEC CYCLE DE NIVEAU HAUT/BAS)
	------------------------------------------------------------------------*/
	void MoveToGround() {
		if ( fl_stable ) {
			return;
		}
		var pt = world.GetGround(cx, cy);
		MoveToCase(pt.x, pt.y);
	}


	// *** STEPPING

	/*------------------------------------------------------------------------
	EX�CUTE LES COLLISIONS ENTRE ENTIT�S
	------------------------------------------------------------------------*/
	protected virtual void CheckHits() {
		var l = GetByType(Data.ENTITY);
		for (var i=0;i<l.Count;i++) {
			if ( !l[i].fl_kill & !fl_kill & this.HitBound(l[i]) & l[i].uniqId!=this.uniqId ) {
				this.Hit(l[i]);
				l[i].Hit(this);
			}
		}
	}

	/*------------------------------------------------------------------------
	PR�FIXE DU STEPPING
	------------------------------------------------------------------------*/
	protected virtual void Prefix() {
		// do nothing
	}

	/*------------------------------------------------------------------------
	INFIXE DU STEPPING (CHANGEMENT DE CASE)
	------------------------------------------------------------------------*/
	protected virtual void Infix() {
		CheckHits();

		var cid = world.GetCase(cx, cy);

		// T�l�portation
		if (fl_teleport & !fl_kill) {
			if (cid == Data.FIELD_TELEPORT) {
				var start = world.GetTeleporter(this, cx, cy);
				if ( start!=null ) {
					bool fl_rand = false;
					var target = world.GetNextTeleporter(start, ref fl_rand);
					game.fxMan.AttachFx(x,y-Data.CASE_HEIGHT,"hammer_fx_pop");
					if (target.direction==Data.HORIZONTAL) {
						if (fl_rand) {
							MoveTo(target.centerX-Data.CASE_WIDTH*0.5f, target.centerY);
						}
						else {
							MoveTo(target.centerX+Entity.x_ctr(cx)-start.centerX, target.centerY);
						}
					}
					else {
						if (fl_rand) {
							MoveTo(target.centerX, target.centerY);
						}
						else {
							MoveTo(target.centerX, target.centerY+Entity.y_ctr(cy)-start.centerY);
						}
					}
					game.fxMan.AttachFx(x,y-Data.CASE_HEIGHT,"hammer_fx_shine");
					game.soundMan.PlaySound("sound_teleport",Data.CHAN_FIELD);
					fl_stopStepping = true;
					lastTeleporter = target;
					OnTeleport();
				}
			}
			else {
				lastTeleporter = null;
			}
		}

		if (fl_portal & !fl_kill) {
			if (cid==Data.FIELD_PORTAL) {
				if (game.fl_clear) {
					var px = cx;
					var py = cy;
					// Cherche le portal correspondant
					while (world.GetCase(px-1, py) == Data.FIELD_PORTAL) {
						px--;
					}
					while (world.GetCase(px, py-1) == Data.FIELD_PORTAL) {
						py--;
					}

					int? pid = null;
					for (var i=0;i<world.portalList.Count;i++) {
						if (world.portalList[i].cx==px & world.portalList[i].cy==py) {
							pid = i;
						}
					}

					OnPortal(pid);
				}
				else {
					OnPortalRefusal();
				}
			}
		}


		// Bumpers
		if (fl_bump & !fl_kill & cid==Data.FIELD_BUMPER) {
			var fdir = Data.VERTICAL;
			if (world.GetCase(cx-1, cy)==cid | world.GetCase(cx+1, cy)==cid) {
				fdir = Data.HORIZONTAL;
			}

			// Projection verticale
			if (fdir==Data.HORIZONTAL & Mathf.Abs(dy??0)<20) {
				dx = 0;
				dy *= 5;
			}
			// Projection horizontale
			if (fdir==Data.VERTICAL & Mathf.Abs(dx??0)<20) {
				dx *= 7;
				dy = 0;
			}
			OnBump();
		}
	}

	/*------------------------------------------------------------------------
	POSTFIXE
	------------------------------------------------------------------------*/
	protected virtual void Postfix() {
		// Do nothing
	}

	/*------------------------------------------------------------------------
	RECALAGE Y
	------------------------------------------------------------------------*/
	protected void Recal() {
		y = Entity.y_ctr(Entity.y_rtc(y));
		UpdateCoords();
		while ( world.GetCase(cx, cy)==Data.GROUND ) {
			y-=Data.CASE_HEIGHT;
			UpdateCoords();
		}
	}

	/*------------------------------------------------------------------------
	CALCUL DE STEPS
	------------------------------------------------------------------------*/
    struct step {
        public float total;
        public float dx;
        public float dy;
    }
	step CalcSteps(float dxStep, float dyStep) {
		var dxTotal = dxStep*Time.fixedDeltaTime;
		var dyTotal = dyStep*Time.fixedDeltaTime;
		var total = Mathf.Ceil(Mathf.Abs(dxTotal)/Data.STEP_MAX);
		total = Mathf.Max(total, Mathf.Ceil(Mathf.Abs(dyTotal)/Data.STEP_MAX) );

        step res = new step();

		res.total = total;
		res.dx = dxTotal/total;
		res.dy = dyTotal/total;

        return res;
	}


	/*------------------------------------------------------------------------
	AUTORISE L'APPLICATION DU PATCH COLLISION AU SOL (ESCALIERS)
	------------------------------------------------------------------------*/
	protected virtual bool NeedsPatch() {
		return false;
	}


	// *** EVENTS

	/*------------------------------------------------------------------------
	EVENT: MORT
	------------------------------------------------------------------------*/
	protected virtual void OnKill() {
		// do nothing
	}

	/*------------------------------------------------------------------------
	EVENT: TOMBE SOUS LA LIGNE DU BAS
	------------------------------------------------------------------------*/
	protected virtual void OnDeathLine() {
		// do nothing
	}

	/*------------------------------------------------------------------------
	EVENT: BLOQUE CONTRE UN MUR
	------------------------------------------------------------------------*/
	protected virtual void OnHitWall() {
		dx = 0;
	}

	/*------------------------------------------------------------------------
	EVENT: ATTERISSAGE
	------------------------------------------------------------------------*/
	protected virtual void OnHitGround(float height) {
		fl_stable = true;
		dy = 0;
		Recal();
	}

	/*------------------------------------------------------------------------
	EVENT: TOUCHE LE PLAFOND
	------------------------------------------------------------------------*/
	protected virtual void OnHitCeil() {
		dy=0;
	}


	/*------------------------------------------------------------------------
	EVENT: T�L�PORTATION
	------------------------------------------------------------------------*/
	protected virtual void OnTeleport() {
		// do nothing
	}


	/*------------------------------------------------------------------------
	EVENT: T�L�PORTATION PORTAL
	------------------------------------------------------------------------*/
	protected virtual void OnPortal(int? pid) {
		// do nothing
	}


	/*------------------------------------------------------------------------
	EVENT: PORTAIL FERM�
	------------------------------------------------------------------------*/
	protected virtual void OnPortalRefusal() {
		// do nothing
	}


	/*------------------------------------------------------------------------
	EVENT: BUMPER
	------------------------------------------------------------------------*/
	protected virtual void OnBump() {
		// do nothing
	}


	// *** UPDATES

	/*------------------------------------------------------------------------
	MISE � JOUR GRAPHIQUE
	------------------------------------------------------------------------*/
	public override void EndUpdate() {
		if (fl_hitBorder) { // patch contre les sorties d'�cran lat�rales
			if ( x<Data.BORDER_MARGIN ) x=Data.BORDER_MARGIN;
			if ( x>=Data.GAME_WIDTH-Data.BORDER_MARGIN ) x=Data.GAME_WIDTH-Data.BORDER_MARGIN-1;
		}

		if (game.fl_aqua) {
			if (!IsType(Data.FX) & (dx!=0 | dy!=0)) {
				if (Random.Range(0, 100)<=10 ) {
					game.fxMan.InGameParticles(
						Data.PARTICLE_BUBBLE,
						x+Random.Range(0, Data.CASE_WIDTH) * (Random.Range(0, 2)*2-1),
						y+Random.Range(0, Data.CASE_HEIGHT),
						1
					);
				}
			}
		}
		base.EndUpdate();
	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	public override void Update() {
		base.Update();

		if (!fl_physics) {
			return;
		}

		UpdateCoords();

		// Vent
		if (fl_wind) {
			if (fl_stable & game.fl_wind) {
				dx += game.windSpeed*Time.fixedDeltaTime;
			}
		}

		// Chute
		if (fl_stable) {
			if (dy!=0 | world.GetCase(fcx, fcy)!=Data.GROUND) {
				fl_stable = false;
			}
		}

		if (dx!=0 | dy!=0 | !fl_stable) {
			int step;

			// Gravit�
			if (!fl_skipNextGravity & !fl_stable & fl_gravity) {

				// facteur de correction pour les tmods extr�mes
				float patchFactor = 1.0f;
				if (Time.fixedDeltaTime>=2) {
					patchFactor = 1.1f;
				}

				if (dy<0) {
					dy += gravityFactor * Data.GRAVITY * Time.fixedDeltaTime * patchFactor;
				}
				else {
					if (fallStart==null) {
						fallStart = y;
					}
					if (game.fl_aqua & !fl_strictGravity) {
						dy += 0.3f * fallFactor * Data.FALL_SPEED * Time.fixedDeltaTime * patchFactor;
					}
					else {
						dy += fallFactor * Data.FALL_SPEED * Time.fixedDeltaTime * patchFactor;
					}
				}
			}
			fl_skipNextGravity = false;

			Prefix();

			var stepInfos = CalcSteps(dx??0, dy??0);
			step=0;

			// D�but du stepping
			while (!fl_stopStepping & step < stepInfos.total) {
				var ocx = cx;
				var ocy = cy;
				var ofcx = fcx;
				var ofcy = fcy;
				var ox = x;
				var oy = y;
				oldX = x;
				oldY = y;

				x += stepInfos.dx;
				y += stepInfos.dy;
				UpdateCoords();

				// Patch p�n�tration dans les murs
				if (fl_hitWall) {
					var fl_hasHitWall = false;
					if (dx>0 & world.GetCase(Entity.x_rtc(ox+Data.CASE_WIDTH*0.5f), Entity.y_rtc(oy))>0) {
						fl_hasHitWall = true;
					}
					if (dx<0 & world.GetCase(Entity.x_rtc(ox-Data.CASE_WIDTH*0.5f), Entity.y_rtc(oy))>0) {
						fl_hasHitWall = true;
					}
					if (fl_hasHitWall) {
						x=ox;
						stepInfos.dx = 0;
						UpdateCoords();
						OnHitWall();
					}
				}

				// Collision horizontale
				if ((fl_hitBorder & (x<Data.BORDER_MARGIN | x>=Data.GAME_WIDTH-Data.BORDER_MARGIN)) |
					(fl_hitWall & world.GetCase(cx, Entity.y_rtc(oy))>0)) {
					x = ox;
					stepInfos.dx = 0;
					UpdateCoords();
					OnHitWall();
				}


				// Patch travers�e de murs par le haut
				if (fl_hitWall & stepInfos.dy>0 & !fl_kill) {
					if (world.GetCase(Entity.rtc(ox,oy+Mathf.Floor(Data.CASE_HEIGHT/2)))!=Data.WALL & world.GetCase(fcx, fcy)==Data.WALL) {
						x = ox;
						stepInfos.dx = 0;
						UpdateCoords();
						OnHitWall();
					}
				}

				// Atterrissage
				if (fl_hitGround & stepInfos.dy>=0) {
					if (world.GetCase(Entity.rtc(ox,oy+Mathf.Floor(Data.CASE_HEIGHT/2)))!=Data.GROUND & world.GetCase(fcx, fcy)==Data.GROUND) {
						if (world.CheckFlag(new Vector2Int(fcx, fcy), Data.IA_TILE)) {
							if (fl_skipNextGround) {
								fl_skipNextGround = false;
							}
							else {
								stepInfos.dy = 0;
								OnHitGround(y-fallStart??0);
								fallStart = null;
								UpdateCoords();
							}
						}
					}
				}

				// Plafond
				if (fl_hitCeil & stepInfos.dy<=0) {
					if (world.GetCase(Entity.rtc(ox,oy-Mathf.Floor(Data.CASE_HEIGHT/2)))<=0 & world.GetCase(Entity.rtc(x,y-Mathf.FloorToInt(Data.CASE_HEIGHT/2)))>0) {
						stepInfos.dy = 0;
						OnHitCeil();
						UpdateCoords();
					}
				}

				// Changement de case
				if ( ocx!=cx || ocy!=cy ) {
					var fl_patch = false;

					// Patch d'entr�e dans un sol avec air jump
					if (fl_hitGround & ocy<cy) {
						if (NeedsPatch()) {
							if (world.GetCase(ocx, ocy)<=0 & dy>0 & world.GetCase(cx, cy)>0 & cy<Data.LEVEL_HEIGHT) {
								x = Entity.x_ctr(ocx);
								y = Entity.y_ctr(ocy);
								stepInfos.dy=0;
								UpdateCoords();
								OnHitGround(y-fallStart??0);
								fl_patch = true;
							}
						}
					}

					// Patch entr�e dans un sol (coins en diagonal)
					if (fl_hitGround & dy>=0 & ocx!=cx & ocy!=cy) {
						if (world.GetCase(ocx, ocy)<=0 & world.GetCase(cx, cy)==Data.GROUND) {
							x = Entity.x_ctr(ocx);
							y = Entity.y_ctr(ocy);
							stepInfos.dx=0;
							UpdateCoords();
							OnHitWall();
							fl_patch = true;
						}
					}
					if (!fl_patch) {
						TRem(ocx,ocy);
						Infix() ; // appel infixe
						UpdateCoords();
						TAdd(cx,cy);
					}
				}
				step++;
			}
			// Fin du stepping
		}

		fl_stopStepping = false;
		Postfix();

		// Frictions
		if (fl_friction) {
			if ((game.fl_ice | fl_slide) & fl_stable) {
				if (slideFriction==null) {
					dx *= game.sFriction;
				}
				else {
					dx *= Mathf.Pow(slideFriction??1, Time.fixedDeltaTime);
				}
			}
			else {
				dx *= game.xFriction;
				dy *= game.yFriction;
			}
		}
		if (Mathf.Abs(dx??0)<=0.2) {
			dx = 0;
		}
		if (Mathf.Abs(dy??0)<=0.2) {
			dy = 0;
		}

		// Mort
		if ( y>=Data.DEATH_LINE ) {
			OnDeathLine();
		}
	}
}
