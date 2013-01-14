#define GameComponents
#define Threading
#define Audio
#define XACT

using System;
using System.Text;
using System.Threading;
using System.Collections.ObjectModel;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using Xbox360IndieGameDesign;

namespace Xbox360DeepDive {
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Game {
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		SpriteFont ScoreFont;
		GameStates CurrentState;
		TimeSpan GameOverTime, TimeToAsteroid;

		IControlState GamePadState;

#if GameComponents
		Player m_Player;
		Collection<Bullet> m_Bullets;
		Collection<Asteroid> m_Asteroids;
#endif
#if Audio
		SoundEffect Pew, Die, Boom;
#if XACT
		///////////////////////////////////////////////////////
		// See README to understand why these is commented out.
		///////////////////////////////////////////////////////

		//Cue _Background;
		//WaveBank _WaveBank;
		//SoundBank _SoundBank;
		//AudioEngine _AudioEngine;
#endif
#endif
		protected override void OnDeactivated( object sender, EventArgs args ) {
			if( CurrentState == GameStates.Playing ) {
				CurrentState = GameStates.Paused;
			}
			base.OnDeactivated( sender, args );
		}

		public Game1() {
			graphics = new GraphicsDeviceManager( this );
			Content.RootDirectory = "Content";
			CurrentState = GameStates.Loading;

#if GameComponents
			m_Player = null;
			m_Bullets = new Collection<Bullet>();
			m_Asteroids = new Collection<Asteroid>();

			this.Components.ComponentAdded += new EventHandler<GameComponentCollectionEventArgs>( ComponentAdded );
			this.Components.ComponentRemoved += new EventHandler<GameComponentCollectionEventArgs>( ComponentRemoved );
#endif
		}

#if GameComponents
		void ComponentAdded( object sender, GameComponentCollectionEventArgs e ) {
		    if( e.GameComponent is Player ) {
		        if( m_Player != null ) {
		            m_Player.Dispose();
		        }

		        m_Bullets.Clear();
		        m_Asteroids.Clear();
		        m_Player = e.GameComponent as Player;
		    } else if( e.GameComponent is Bullet ) {
#if Audio
				Pew.Play();
#endif
				m_Bullets.Add( e.GameComponent as Bullet );
			} else if( e.GameComponent is Xbox360IndieGameDesign.Asteroid ) {
				m_Asteroids.Add( e.GameComponent as Xbox360IndieGameDesign.Asteroid );
			}
		}

		void ComponentRemoved( object sender, GameComponentCollectionEventArgs e ) {
			if( e.GameComponent is Bullet ) {
				m_Bullets.Remove( e.GameComponent as Bullet );
			} else if( e.GameComponent is Xbox360IndieGameDesign.Asteroid ) {
				m_Asteroids.Remove( e.GameComponent as Xbox360IndieGameDesign.Asteroid );
			}
		}
#endif

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize() {
			GamePadState = new ControllerOneControlState( this );
			Components.Add( GamePadState );
#if Threading
			Thread _StarThread = new Thread( CreateStars );
			_StarThread.Start();
#endif
			base.Initialize();
		}

#if Threading
		private void CreateStars() {
			Thread.CurrentThread.SetProcessorAffinity( 5 );

			for( int _lcv = 0; _lcv < 100; ++_lcv ) {
				Star _Star = new Star( this );
				_Star.Enabled = false;
				_Star.Visible = false;
				Components.Add( _Star );
				_Star.Initialize();
			}
		}

		private void EnableStars() {
			Thread.CurrentThread.SetProcessorAffinity( 5 );

			foreach( GameComponent _Component in Components ) {
				if( _Component is Star ) {
					_Component.Enabled = true;
					( (DrawableGameComponent)_Component ).Visible = true;
				}
			}
		}

#if Audio
		private void LoadAudio() {
		    Thread.CurrentThread.SetProcessorAffinity( 3 );

		    Pew = this.Content.Load<SoundEffect>( "Sounds\\Pew" );
		    Die = this.Content.Load<SoundEffect>( "Sounds\\Die" );
		    Boom = this.Content.Load<SoundEffect>( "Sounds\\Boom" );
#if XACT
			//_AudioEngine = new AudioEngine( "XACT\\Xbox360DeepDive.xgs" );
			//_WaveBank = new WaveBank( _AudioEngine, "XACT\\Wave Bank.xwb" );
			//_SoundBank = new SoundBank( _AudioEngine, "XACT\\Sound Bank.xsb" );
#endif
		}
#endif
#endif

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent() {
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch( GraphicsDevice );

#if Threading
#if Audio
			Thread _SoundThread = new Thread( LoadAudio );
			_SoundThread.Start();
#endif
#endif
			ScoreFont = this.Content.Load<SpriteFont>( "ScoreFont" );
			CurrentState = GameStates.Splash;

			base.LoadContent();
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update( GameTime gameTime ) {
			// Allows the game to exit
			if( GamePadState.Exit )
				Exit();

			switch( CurrentState ) {
				case GameStates.Menu:
					if( GamePadState.Fire ) {
						Components.Add( new Player( this, GamePadState ) );
						TimeToAsteroid = TimeSpan.Zero;
#if XACT
						//_Background = _SoundBank.GetCue( "Intro" );
						//_Background.Play();
#endif
						CurrentState = GameStates.Playing;
					}
					break;
				case GameStates.GameOver:
					if( GameOverTime < TimeSpan.Zero )
						CurrentState = GameStates.Menu;

					GameOverTime -= gameTime.ElapsedGameTime;
					break;
				case GameStates.Paused:
#if XACT
					//if( _Background.IsPlaying )
					//    _Background.Pause();
#endif

					if( GamePadState.Pause ) {
#if XACT
						//_Background.Resume();
#endif
						CurrentState = GameStates.Playing;
					}
					break;
				case GameStates.Playing:
#if XACT
					//if( _Background.IsStopped ) {
					//    _Background = _SoundBank.GetCue( "Main" );
					//    _Background.Play();
					//}
#endif
					if( TimeToAsteroid > TimeSpan.Zero ) {
						TimeToAsteroid -= gameTime.ElapsedGameTime;
					} else {
						TimeToAsteroid = new TimeSpan( 0, 0, 0, 0, 500 );
						Components.Add( new Xbox360IndieGameDesign.Asteroid( this ) );
					}

					for( int _AsteroidIndex = 0; _AsteroidIndex < m_Asteroids.Count; ++_AsteroidIndex ) {
						bool _DestroyAsteroid = false;
						Xbox360IndieGameDesign.Asteroid _Asteroid = m_Asteroids[_AsteroidIndex];
						foreach( Bullet _Bullet in m_Bullets ) {
							if( _Asteroid.Position.Intersects( _Bullet.Position ) ) {
#if Audio
								Boom.Play();
#endif
								--_AsteroidIndex;
								m_Player.Score += 1000;
								_DestroyAsteroid = true;
								Components.Remove( _Bullet );
								Components.Remove( _Asteroid );
								break;
							}
						}

						if( !_DestroyAsteroid ) {

							if( _Asteroid.Position.Intersects( m_Player.Position ) ) {
								Components.Remove( _Asteroid );
								try {
#if Audio
									Die.Play();
									Boom.Play();
#endif
									m_Player.Score += 1000;
									m_Player.LoseLife();
								} catch( GameOverException ) {
#if XACT
									//_Background.Stop( AudioStopOptions.Immediate );
									//_Background = _SoundBank.GetCue( "Outro" );
									//_Background.Play();
#endif
									GameOverTime = new TimeSpan( 0, 0, 5 );
									CurrentState = GameStates.GameOver;
								}
								break;
							}
						}
					}
					break;
			}

			base.Update( gameTime );
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw( GameTime gameTime ) {
			if( CurrentState == GameStates.Splash ) {
#if Threading
				Thread _StarThread = new Thread( EnableStars );
			    _StarThread.Start();
#endif
				CurrentState = GameStates.Menu;
			}

			GraphicsDevice.Clear( Color.Black );
			base.Draw( gameTime );

			spriteBatch.Begin();

			StringBuilder _Status = new StringBuilder();
			//_Status.AppendFormat( "Framerate: {0:F2}", 1000.0 / gameTime.ElapsedGameTime.TotalMilliseconds );
			//_Status.AppendLine();
			
			if( m_Player != null ) {
				_Status.Append( "Score: " );
				_Status.AppendLine( m_Player.Score.ToString() );
			}

			switch( CurrentState ) {
				case GameStates.Menu:
					_Status.Append( "Press A to Begin" );
					spriteBatch.DrawString( ScoreFont, _Status, Vector2.Zero, Color.Green );
					break;
				case GameStates.GameOver:
					_Status.Append( "Game Over" );
					spriteBatch.DrawString( ScoreFont, _Status, Vector2.Zero, Color.Red );
					break;
				case GameStates.Paused:
					_Status.Append( "Paused" );
					spriteBatch.DrawString( ScoreFont, _Status, Vector2.Zero, Color.Red );
					break;
				case GameStates.Playing:
					_Status.Append( "Lives: " );
					_Status.Append( m_Player.Lives.ToString() );
					spriteBatch.DrawString( ScoreFont, _Status, Vector2.Zero, Color.White );
					spriteBatch.Draw( m_Player.Sprite, m_Player.Position, Color.White );

					foreach( Bullet _Bullet in m_Bullets )
						spriteBatch.Draw( _Bullet.Sprite, _Bullet.Position, Color.White );

					foreach( Xbox360IndieGameDesign.Asteroid _Asteroid in m_Asteroids )
						spriteBatch.Draw( _Asteroid.Sprite, _Asteroid.Position, Color.White );
					break;
			}

			spriteBatch.End();
		}

	}
}
