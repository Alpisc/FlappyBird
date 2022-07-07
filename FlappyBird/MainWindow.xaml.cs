using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using System.Windows.Threading;

namespace FlappyBird
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //Variablen initiieren
        DispatcherTimer gameTimer = new DispatcherTimer();

        Random rnd = new Random();

        Rect flappyBirdHitBox;

        DispatcherTimer pauseTimer = new DispatcherTimer();
        int pauseTicks;

        double score;
        double highScore = 0;
        int gravity = 1; //def 8
        int velocity;
        int tmpUp;
        int distance;
        int difficulty;
        bool gameOver;
        bool hasJumped;
        bool endlessmode = false;
        bool gamePause;
        bool pauseButtonPressed;
        bool casualMode = false;
        bool gameReady = false;
        bool gameStarted = false;

        public MainWindow()
        {
            InitializeComponent();

            gameTimer.Tick += MainEventTimer;
            gameTimer.Interval = TimeSpan.FromMilliseconds(20);

            pauseTimer.Tick += PauseTimerContinue;
            pauseTimer.Interval = TimeSpan.Zero;
            StartGame();
        }

        private void StartGame()
        {
            gameReady = false;
            gameStarted = false;

            MyCanvas.Focus(); //spiel wird gefokust


            //wichtige variablen neu setzen
            int temp = 300;

            score = 0; //start score

            velocity = 0;

            distance = 0;

            difficulty = 0;

            gameOver = false;

            gamePause = false;

            hasJumped = false;

            pauseButtonPressed = false;

            pauseTicks = 0;

            txtScore.Content = "Score: 0";


            
            winBox.Visibility = Visibility.Hidden;
            winDisplay.Visibility = Visibility.Hidden;
            winRetry.Visibility = Visibility.Hidden;
            
            /*
            foreach (var x in MyCanvas.Children.OfType<Label>()) // jedes label mit "win" im namen wird versteckt
            {
                if (x.Name.Contains("win"))
                {
                    x.Visibility = Visibility.Hidden;
                }
            }
            */

            Canvas.SetTop(flappyBird, 190); //flappybird wird auf den startpunkt gesetzt

            foreach (var x in MyCanvas.Children.OfType<Image>()) // erstmalige wolken und röhren generation
            {
                if ((string)x.Tag == "obs1")
                {
                    Canvas.SetLeft(x, 500);
                }
                if ((string)x.Tag == "obs2")
                {
                    Canvas.SetLeft(x, 800);
                }
                if ((string)x.Tag == "obs3")
                {
                    Canvas.SetLeft(x, 1100);
                }

                if ((string)x.Tag == "cloud")
                {
                    Canvas.SetLeft(x, 300 + temp);
                    temp = 800;
                }

                gameReady = true;
            }
        }

        private void MainEventTimer(object sender, EventArgs e) //wird jeden tick aufgerufen
        {
            flappyBirdHitBox = new Rect(Canvas.GetLeft(flappyBird), Canvas.GetTop(flappyBird), flappyBird.Width - 5, flappyBird.Height - 5); //hitbox von flappy, kann verkleinert werden um einfacher zu sein

            velocity += gravity; // fall animation

            Canvas.SetTop(flappyBird, Canvas.GetTop(flappyBird) + velocity); // + gravity

            if ((difficulty >= 10 || score >= 100) && endlessmode == false)
            {
                WinGame(); //spiel pausiert + Gewinn bildschirm kommt
            }

            if (Canvas.GetTop(flappyBird) < -10 || Canvas.GetTop(flappyBird) > 458)
            {
                EndGame(); //spiel pausiert + nutzer wird aufgefordert neu zu starten
            }

            tmpUp = rnd.Next(-320, -160); //wo die obere röhre generiert werden soll (wird benötigt um den abstand richtig zu machen)

            foreach (var x in MyCanvas.Children.OfType<Image>()) //röhren werden an zufälligen positionen plaziert
            {
                if ((string)x.Tag == "obs1" || (string)x.Tag == "obs2" || (string)x.Tag == "obs3")
                {
                    Canvas.SetLeft(x, Canvas.GetLeft(x) - 5);

                    if (Canvas.GetLeft(x) < -100)
                    {
                        Canvas.SetLeft(x, 800);

                        /*
                         * zufalls positionen
                         */
                        if (x.Name.Contains("Top"))
                        {
                            Canvas.SetTop(x, tmpUp);
                        }

                        if (x.Name.Contains("Bottom"))
                        {
                            if (casualMode == false)
                            {
                                Canvas.SetTop(x, tmpUp + 500 - distance);
                            }
                            else
                            {
                                Canvas.SetTop(x, tmpUp + 500);
                            }
                        }

                        //.5 weil 2 röhren auf einmal kommen -> 1 punkt pro 2 röhren
                        score += .5;

                        txtScore.Content = $"Score: {score}";

                        if (casualMode == false)
                        {
                            if ((score + 2) % 10 == 0)
                            {
                                //geht nicht
                                distance += 5;// distanz wird erhöht bevor der spieler soweit ist damit die röhren wenn er soweit ist, auf der richtigen position sind
                            }

                            if (score % 10 == 0 && score != 0)
                            {
                                score = 0;
                                difficulty++; //maximale difficulty = 9
                                txtDifficulty.Content = $"Difficulty: {difficulty}";
                                
                            }

                            if (highScore < difficulty)
                            {
                                highScore = difficulty; //wird neu gesetzt falls highscore übertroffen wird
                                txtHighScore.Content = $"Highscore: {highScore}";
                            }
                        }
                        else
                        {
                            if (highScore < score)
                            {
                                highScore = score;
                                txtHighScore.Content = $"Highscore: {highScore}";
                            }
                        }
                    }

                    Rect pipeHitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height); //hitbox von jeder röhre

                    if (flappyBirdHitBox.IntersectsWith(pipeHitBox)) //falls flappy die röhre berührt
                    {
                        EndGame();
                    }
                }

                if (x.Name.Contains("cloud"))
                {
                    Canvas.SetLeft(x, Canvas.GetLeft(x) - 1);

                    if (Canvas.GetLeft(x) < -250)
                    {
                        Canvas.SetLeft(x, 550);
                    }
                }
            }
        }

        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && gameOver == false && hasJumped == false && gamePause == false)
            {
                if (gameReady == true && gameStarted == false)
                {
                    gameTimer.Start(); // spiel startet
                    gameStarted = true;
                }

                flappyBird.RenderTransform = new RotateTransform(-20, flappyBird.Width / 2, flappyBird.Height / 2);
                // Canvas.SetTop(flappyBird, Canvas.GetTop(flappyBird) - 50); //gravity = -8; // old fly/jump thing

                velocity = -7; //um leicht nach oben zu kommen

                Canvas.SetTop(flappyBird, Canvas.GetTop(flappyBird) + velocity); //flappy wird auf seine neue position gesetzt

                hasJumped = true; //damit man nicht durchgehend fliegen kann
            }

            if (e.Key == Key.Escape && gamePause == false)
            {
                PauseGame();
                pauseButtonPressed = true; //damit das pausier fenser nicht direkt geschlossen wird
            }

            if (e.Key == Key.Escape && gamePause == true && pauseButtonPressed == false)
            {
                pauseTicks = 0;
                ResumeGame();
            }

            if (e.Key == Key.R && gameOver == true)
            {
                //if (highScore < score) highScore = score; //gameOver Update only
                StartGame();
            }
        }

        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            flappyBird.RenderTransform = new RotateTransform(5, flappyBird.Width / 2, flappyBird.Height / 2);
            //gravity = 8; old jump/fly thing

            hasJumped = false; //damit beim nächsten drücken der nutzer wieder springen kann
            pauseButtonPressed = false; //damit man das pausier menü schließen kann
        }

        private void EndGame()
        {
            gameTimer.Stop(); //spiel wird beentet
            gameOver = true;
            txtScore.Content += " | Game Over !! Press R to try again";
        }

        private void WinGame()
        {
            gameTimer.Stop();

            winBox.Visibility = Visibility.Visible;
            winDisplay.Visibility = Visibility.Visible;
            winRetry.Visibility = Visibility.Visible;

            /*
            foreach (var x in MyCanvas.Children.OfType<Label>())
            {
                if (x.Name.Contains("win")) //alle label mit win im namen sichtbar machen
                {
                    x.Visibility = Visibility.Visible;
                }
            }
            */

            gameOver = true;
        }

        private void PauseGame()
        {
            gameTimer.Stop();
            gamePause = true;

            /*
            foreach (var x in MyCanvas.Children.OfType<Label>())
            {
                if (x.Name.Contains("pause") || x.Name.Contains("Endless") || x.Name.Contains("CasualMode")) //alle label mit "pause" oder "Endless" im namen sichtbar machen
                {
                    x.Visibility = Visibility.Visible;
                }
            }
            */
            pauseBackground.Visibility = Visibility.Visible;
            pauseText.Visibility = Visibility.Visible;

            txtEndless.Visibility = Visibility.Visible;
            btnEndless.Visibility = Visibility.Visible;

            btnCasualMode.Visibility = Visibility.Visible;
            txtCasualMode.Visibility = Visibility.Visible;

            foreach (var x in MyCanvas.Children.OfType<Button>())
            {
                if (x.Name.Contains("Endless") || x.Name.Contains("ResetHighScore") || x.Name.Contains("CasualMode")) //alle knöpfe mit "pause" oder "ResetHighScore" im namen sichtbar machen
                {
                    x.Visibility = Visibility.Visible;
                }
            }
        }

        private void ResumeGame()
        {
            foreach (var x in MyCanvas.Children.OfType<Label>())
            {
                if (x.Name.Contains("pause") || x.Name.Contains("Endless") || x.Name.Contains("CasualMode")) //alle label mit "pause" oder "Endless" im namen verbergen
                {
                    x.Visibility = Visibility.Hidden;
                }
            }
            foreach (var x in MyCanvas.Children.OfType<Button>())
            {
                if (x.Name.Contains("Endless") || x.Name.Contains("ResetHighScore") || x.Name.Contains("CasualMode")) //alle knöpfe mit "pause" oder "ResetHighScore" im namen verbergen
                {
                    x.Visibility = Visibility.Hidden;
                }
            }

            if(gameOver == false)
            {
                pauseTimer.Interval = TimeSpan.Zero;
                pauseTimer.Start();
            }
            else
            {
                gamePause = false;
            }
        }

        private void btnEndless_Click(object sender, RoutedEventArgs e)
        {
            endlessmode = !endlessmode; //endless mode switch

            if (endlessmode == true)
            {
                txtEndless.Content = "On";
            }
            else
            {
                txtEndless.Content = "Off";
            }
        }

        private void btnResetHighScore_Click(object sender, RoutedEventArgs e)
        {
            highScore = 0; //highscore wird resetet
        }

        private void btnCasualMode_Click(object sender, RoutedEventArgs e)
        {
            casualMode = !casualMode; // true -> false | false -> true

            if (casualMode == true)
            {
                txtCasualMode.Content = "On";
                txtDifficulty.Content = "";
            }
            else
            {
                txtCasualMode.Content = "Off";
                txtDifficulty.Content = $"Difficutly: {difficulty}";
            }

            highScore = 0;
        }

        private void PauseTimerContinue(object sender, EventArgs e)
        {
            pauseTimer.Interval = TimeSpan.FromSeconds(1);

            pResume.Content = 3 - pauseTicks;

            pResume.Visibility = Visibility.Visible;

            if (pauseTicks == 3)
            {
                pauseTimer.Stop();
                pResume.Visibility = Visibility.Hidden;
                gamePause = false;
                pauseTicks = 0;
                gameTimer.Start();
                return;
            }

            pauseTicks++;
        }
    }
}
