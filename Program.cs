using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
namespace Qwixx
{
    class Qwixx
    {
        static int red, blue, yellow, green, white1, white2, players, activePlayer;
        static int[] dice;
        static string[] diceNames = new string[] { "White: ", "White: ", "Red: ", "Yellow: ", "Green: ", "Blue: " };
        static Random rand = new Random();
        static Card card1, card2, card3, card4;
        static Card[] cards;
        static Dictionary<string, int> responseToInt = new Dictionary<string, int>()
        {
            { "R", 0 },
            { "Y", 1 },
            { "G", 2 },
            { "B", 3 }
        };
        static Dictionary<string, string> responseToString = new Dictionary<string, string>()
        {
            { "R", "Red" },
            { "Y", "Yellow" },
            { "G", "Green" },
            { "B", "Blue" }
        };
        static List<int> lockedRows = new List<int>();


        static void Main(string[] args)
        {
        Start:
            Console.WriteLine("How Many Players?");
            int p = 1000000;
            if(!int.TryParse(Console.ReadLine(), out p) || p < 1 || p > 4)
            {
                Console.WriteLine("Invalid Input!");
                goto Start;
            }
            initGame(p, false);
        }

        static void initGame(int p, bool ai)
        {
            players = p;
            Console.WriteLine("Initiating Game With " + players + " Players...");
            activePlayer = 1; 
            switch (p)
            {
                case 1:
                    card1 = new Card(1);
                    break;
                case 2:
                    card1 = new Card(1);
                    card2 = new Card(2);
                    break;
                case 3:
                    card1 = new Card(1);
                    card2 = new Card(2);
                    card3 = new Card(3);
                    break;
                case 4:
                    card1 = new Card(1);
                    card2 = new Card(2);
                    card3 = new Card(3);
                    card4 = new Card(4);
                    break;
                default:
                    break;
            }
            cards = new Card[] { card1, card2, card3, card4 };
            Thread.Sleep(1000);
            turn();
        }

        static void turn()
        {
            roll();
            bool passed = false;
            int whiteSum = dice[0] + dice[1];
            List<int> pRed = new List<int>(), pYellow = new List<int>(), pGreen = new List<int>(), pBlue = new List<int>();
            pRed.Add(white1 + red);
            pRed.Add(white2 + red);
            pYellow.Add(white1 + yellow);
            pYellow.Add(white2 + yellow);
            pGreen.Add(white1 + green);
            pGreen.Add(white2 + green);
            pBlue.Add(white1 + blue);
            pBlue.Add(white2 + blue);
            List<int>[] lists = new List<int>[] { pRed, pYellow, pGreen, pBlue };
            List<string> toBeLocked = new List<string>();
            
            Regex rx = new Regex(@"^([GRBY])$");
            Console.Clear();
            Console.WriteLine("It is player " + activePlayer + "'s turn, however, boards are marked in player order regardless of whose turn it is.");
            for (int i=0; i<players; i++)
            {
                cards[i].printCard();
                printDice();
                bool yes = i + 1 == activePlayer;
                if (yes)
                {
                    Console.WriteLine("You are the active player. In addition to marking off the sum of the white dice, you will also have the option of marking off the sum of one white die and one colored die.");
                }
                Input:
                Console.WriteLine("Player "+(i+1)+", where should the white dice sum (" + whiteSum + ") be marked off? Type 'R', 'Y', 'B', or 'G' to mark a row, or 'N' to not mark any row.");
                string response = Console.ReadLine().ToUpper().Trim();
                Console.WriteLine("You entered: " + response);
                bool p = false;
                if (response.Equals("N"))
                {
                    if (!passed) { passed = yes; }
                    p = true;
                    Console.WriteLine("You chose not to mark off any row with the sum of the white dice.");
                } else if (rx.IsMatch(response))
                {
                    if (lockedRows.Contains(responseToInt[response]))
                    {
                        Console.WriteLine("That row has been locked!");
                        goto Input;
                    }
                    if (cards[i].getRows()[responseToInt[response]].addMarkedNumber(whiteSum))
                    {
                        Console.WriteLine("The number " + whiteSum + " was marked off of the " + responseToString[response].ToLower() + " row.");
                        if (cards[i].getRows()[responseToInt[response]].getStartsSmall())
                        {
                            if (whiteSum == 12)
                            {
                                cards[i].getRows()[responseToInt[response]].addMarkedNumber(13);
                                toBeLocked.Add(response);
                            }
                        } else
                        {
                            if (whiteSum == 2)
                            {
                                cards[i].getRows()[responseToInt[response]].addMarkedNumber(1);
                                toBeLocked.Add(response);
                            }
                        }
                    } else
                    {
                        Console.WriteLine("The number "+whiteSum+" could not be marked off of the "+responseToString[response].ToLower()+" row.");
                        goto Input;
                    }                          
                } else
                {
                    Console.WriteLine("Invalid input!");
                    goto Input;
                }
                if (!p)
                {
                    Console.Clear();
                    Console.WriteLine("After any marks you made this turn, your board now looks like this:");
                    cards[i].printCard();
                }
                Console.WriteLine("Press [Enter] to move to the next player.");
                while (Console.ReadKey().Key != ConsoleKey.Enter) { }
                Thread.Sleep(100);
                Console.Clear();
            }
            if (toBeLocked.Count > 0)
            {
                foreach (string i in toBeLocked)
                {
                    lockedRows.Add(responseToInt[i]);
                    Console.WriteLine("The " + responseToString[i].ToLower() + " row was locked!");
                }
                if (lockedRows.Count > 1)
                {
                    end(2);
                    return;
                }
                Console.WriteLine("Press [Enter] to continue to player " + activePlayer + "'s second move.");
                while (Console.ReadKey().Key != ConsoleKey.Enter) { }
                Thread.Sleep(100);
            }       
            Console.Clear();
        Input2:
            bool p2 = false;
            cards[activePlayer - 1].printCard();
            printDice();
            Console.WriteLine("You (player " + activePlayer + ") may now mark off an additional number from one row by adding one colored die to one white die. Please enter the row and number you'd like to mark off (as in 'R3' or 'B11', etc) or enter 'N' to not mark any row.");
            if (passed) { Console.WriteLine("Since you passed your last move, passing again would result in a penalty."); }
            string response2 = Console.ReadLine().ToUpper().Trim();
            int l = response2.Length;
            if (l < 4)
            {
                if (response2.Equals("N"))
                {
                    p2 = true;
                    if (passed)
                    {
                        cards[activePlayer-1].addPenalty();
                        int p = cards[activePlayer - 1].getPenalties();
                        Console.WriteLine("Since you did not mark off any numbers this turn, you receive a penalty. You now have " + p + " penalties.");
                        if (p > 3)
                        {
                            end(1);
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine("You chose not to mark off any row with the sum of a white die and a colored die.");
                    }
                }
                else if(l>1)
                {
                    string letter = response2.Substring(0, 1);
                    int num = int.Parse(response2.Substring(1, l - 1));
                    if (!rx.IsMatch(letter) || num > 12 || num < 2)
                    {
                        Console.WriteLine("Invalid input!");
                        goto Input2;
                    }
                    else
                    {
                        if (lists[responseToInt[letter]].Contains(num))
                        {
                            if (lockedRows.Contains(responseToInt[letter]))
                            {
                                Console.WriteLine("That row has been locked!");
                                goto Input2;
                            }
                            if (cards[activePlayer - 1].getRows()[responseToInt[letter]].addMarkedNumber(num))
                            {
                                Console.WriteLine("The number " + num + " was marked off of the " + responseToString[letter].ToLower() + " row.");
                                if (cards[activePlayer - 1].getRows()[responseToInt[letter]].getStartsSmall())
                                {
                                    if (num == 12)
                                    {
                                        cards[activePlayer - 1].getRows()[responseToInt[letter]].addMarkedNumber(13);
                                        lockedRows.Add(responseToInt[letter]);
                                    }
                                }
                                else
                                {
                                    if (num == 2)
                                    {
                                        cards[activePlayer - 1].getRows()[responseToInt[letter]].addMarkedNumber(1);
                                        lockedRows.Add(responseToInt[letter]);
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("The number " + num + " could not be marked off of the " + responseToString[letter].ToLower() + " row.");
                                goto Input2;
                            }
                        }
                        else
                        {
                            Console.WriteLine("The " + responseToString[letter].ToLower() + " die (" + dice[responseToInt[letter]] + ") can not be added to either of the white dice (" + white1 + " or " + white2 + ") to make a " + num + ".");
                            goto Input2;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Invalid input!");
                goto Input2;
            }
            if (!p2)
            {
                Console.Clear();
                Console.WriteLine("After any marks you made this turn, your board now looks like this:");
                cards[activePlayer - 1].printCard();
            }
            Console.WriteLine("Press [Enter] to continue to the next turn.");
            while (Console.ReadKey().Key != ConsoleKey.Enter) { }
            Thread.Sleep(100);
            Console.Clear();
            if (lockedRows.Count > 1)
            {
                end(2);
                return;
            }
            activePlayer++;
            if (activePlayer > players)
            {
                activePlayer = 1;
            }
            turn();
        }

        static void end(int how)//1 = penalties, 2 = two rows locked
        {
            Console.Clear();
            switch (how) {
                case 1:
                    Console.WriteLine("The game ended! One player got 4 penalties.");
                    break;
                case 2:
                    Console.WriteLine("The game ended! Two rows have been locked!");
                    break;
                default:
                    break;
            }
            int[] scores = new int[] { 0, 0, 0, 0 };
            for (int i = 0; i < players; i++)
            {
                Console.WriteLine("Here is Player " + (i + 1) + "'s final board and score:");
                cards[i].printCard();
                scores[i] = cards[i].printScore();
                Console.WriteLine("-------------------------------------------------------------");
            }
        }

        static void roll()
        {
            red = rand.Next(1, 7);
            blue = rand.Next(1, 7);
            yellow = rand.Next(1, 7);
            green = rand.Next(1, 7);
            white1 = rand.Next(1, 7);
            white2 = rand.Next(1, 7);
            dice = new int[] { white1, white2, red, yellow, green, blue };
        }

        static void printDice()
        {
            if (red == 0) return;
            for (int i = 0; i < 6; i++)
            {
                if (lockedRows.Contains(i - 2))
                {
                    continue;
                }
                Console.WriteLine(diceNames[i] + dice[i]);
            }
            
        }

    }

    class Card
    {
        Row[] rows;
        int player;
        int penalties;

        public Card(int p)
        {
            this.rows = new Row[] { new Row(0, true), new Row(1, true), new Row(2, false), new Row(3, false) };
            player = p;
            penalties = 0;
        }
        public int printScore()
        {
            int score = calcScore();

            Console.WriteLine("╔═════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                       Score for Player "+player+"                        ║");
            Console.WriteLine("║ ┌──────────────────────────────────────────────────────────────┐║");
            Console.WriteLine("║ │    "+xToString(0)+ "       " + xToString(1) + "       " + xToString(2) + "       " + xToString(3) + "       " + penalties+"                 │║");
            Console.WriteLine("║ │ R: ↓      Y: ↓      G: ↓      B: ↓      P: ↓      =>  "+scoreToPts(score)+" │║");
            Console.WriteLine("║ │    "+xToPts(0)+ "  +  " + xToPts(1) + "  +  " + xToPts(2) + "  +  " + xToPts(3) + "  -  " + pToPts() + "             │║");
            Console.WriteLine("║ └──────────────────────────────────────────────────────────────┘║");
            Console.WriteLine("╚═════════════════════════════════════════════════════════════════╝");

            return score;
        }
        int calcScore()
        {
            int score = 0;
            for(int i =0; i<4; i++)
            {
                for(int j=1; j < rows[i].getMarkedCount() + 1; j++)
                {
                    score += j;
                }
            }
            score -= penalties * 3;
            return score;
        }
        string scoreToPts(int s)
        {
            return (s < 10 ? s.ToString() + "pts  " : (s<100? s.ToString() + "pts " : s.ToString() + "pts"));
        }
        string xToPts(int r)
        {
            int pts = 0;
            int x = rows[r].getMarkedCount();
            for (int i = 1; i<x+1; i++)
            {
                pts += i;
            }
            return (pts<10? pts.ToString() + "pts " : pts.ToString() + "pts");
        }
        string pToPts()
        {
            int pts = 5 * penalties;
            return (pts < 10 ? pts.ToString() + "pts " : pts.ToString() + "pts");
        }
        string xToString(int r)
        {
            return (rows[r].getMarkedCount().ToString() + "x" + (rows[r].getMarkedCount()<10?" ":""));
        }
        public void printCard()
        {
            Console.WriteLine("╔══════════════════════════════════════════╗");
            Console.WriteLine("║                 Player: "+player+"                ║");
            Console.WriteLine("║ ┌────────────────────────────────┬──────┐║");
            Console.WriteLine("║R│ " + rows[0].printRow() + " │║");
            Console.WriteLine("║ ├────────────────────────────────┼──────┤║");
            Console.WriteLine("║Y│ " + rows[1].printRow() + " │║");
            Console.WriteLine("║ ├────────────────────────────────┼──────┤║");
            Console.WriteLine("║G│ " + rows[2].printRow() + " │║");
            Console.WriteLine("║ ├────────────────────────────────┼──────┤║");
            Console.WriteLine("║B│ " + rows[3].printRow() + " │║");
            Console.WriteLine("║ └────────────────────────────────┴──────┘║");
            Console.WriteLine("║              Penalties: "+penalties+"/4              ║");
            Console.WriteLine("╚══════════════════════════════════════════╝");
        }
        public Row[] getRows()
        {
            return rows;
        }
        public void addPenalty()
        {
            penalties++;
        }
        public int getPenalties()
        {
            return penalties;
        }
    }
    class Row
    {
        int color;
        bool startsSmall;
        List<int> markedNumbers;

        public Row(int c, bool s)
        {
            this.color = c;
            this.startsSmall = s;
            this.markedNumbers = new List<int>();
        }

        public bool addMarkedNumber(int n)
        {
            if (markedNumbers.Contains(n) || (startsSmall ? n < getFurthestRight() : n > getFurthestRight()) || (markedNumbers.Count < 5 && (startsSmall ? n==12 : n==2 ))) 
            { 
                return false; 
            } 
            else
            {
                markedNumbers.Add(n);
                return true;
            }
        }

        public bool getStartsSmall()
        {
            return startsSmall;
        }

        public int getMarkedCount()
        {
            return markedNumbers.Count;
        }

        public int getFurthestRight()
        {
            if (startsSmall)
            {
                switch (markedNumbers.Count)
                {
                    case 0:
                        return 0;
                    case 1:
                        return markedNumbers[0];
                    default:
                        int max = markedNumbers[0];
                        for(int i=1; i<markedNumbers.Count; i++)
                        {
                            if (markedNumbers[i] > max)
                            {
                                max = markedNumbers[i];
                            }
                        }
                        return max;
                }
            } else
            {
                switch (markedNumbers.Count)
                {
                    case 0:
                        return 13;
                    case 1:
                        return markedNumbers[0];
                    default:
                        int min = markedNumbers[0];
                        for (int i = 1; i < markedNumbers.Count; i++)
                        {
                            if (markedNumbers[i] < min)
                            {
                                min = markedNumbers[i];
                            }
                        }
                        return min;
                }
            }
        }

        public string printRow()
        {
            string r = "";
            if (startsSmall)
            {
                for(int i = 2; i<10; i++)
                {
                    if (markedNumbers.Contains(i))
                    {
                        r += " █ ";
                    } else
                    {
                        r += " " + i + " ";
                    }
                }
                for(int i=10; i < 12; i++)
                {
                    if (markedNumbers.Contains(i))
                    {
                        r += " █ ";
                    }
                    else
                    {
                        r += " " + i;
                    }
                }
                r += "   " + (markedNumbers.Contains(12) ? "█  X" : "12 O");
            } 
            else
            {
                for (int i = 12; i > 9; i--)
                {
                    if (markedNumbers.Contains(i))
                    {
                        r += " █ ";
                    }
                    else
                    {
                        r += " " + i;
                    }
                }
                for (int i = 9; i > 2; i--)
                {
                    if (markedNumbers.Contains(i))
                    {
                        r += " █ ";
                    }
                    else
                    {
                        r += " " + i + " ";
                    }
                }
                r += "   " + (markedNumbers.Contains(2) ? "█  X" : "2  O");
            }
            return r;
        }
    }
}
