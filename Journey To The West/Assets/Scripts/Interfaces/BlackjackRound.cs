using System;
using System.Collections.Generic;
using System.Text;

public enum BlackjackOutcome
{
    None,
    PlayerWin,
    DealerWin,
    Push
}

public class BlackjackRound
{
    private static readonly string[] CardRanks =
    {
        "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K"
    };

    private readonly List<string> deck = new List<string>();
    private readonly List<string> playerHand = new List<string>();
    private readonly List<string> dealerHand = new List<string>();
    private readonly Random random = new Random();

    public bool IsGameOver { get; private set; }
    public BlackjackOutcome Outcome { get; private set; }
    public string ResultMessage { get; private set; } = "";

    public BlackjackRound()
    {
        ResetRound();
    }

    public void ResetRound()
    {
        deck.Clear();
        playerHand.Clear();
        dealerHand.Clear();
        IsGameOver = false;
        Outcome = BlackjackOutcome.None;
        ResultMessage = "";

        BuildDeck();
        ShuffleDeck();

        playerHand.Add(DrawCard());
        dealerHand.Add(DrawCard());
        playerHand.Add(DrawCard());
        dealerHand.Add(DrawCard());

        ResolveOpeningBlackjack();
    }

    public void Hit()
    {
        if (IsGameOver) return;

        playerHand.Add(DrawCard());

        if (GetPlayerTotal() > 21)
        {
            IsGameOver = true;
            Outcome = BlackjackOutcome.DealerWin;
            ResultMessage = "You busted. King Modi wins.";
        }
    }

    public void Stand()
    {
        if (IsGameOver) return;

        while (GetDealerTotal() < 17)
        {
            dealerHand.Add(DrawCard());
        }

        IsGameOver = true;
        int playerTotal = GetPlayerTotal();
        int dealerTotal = GetDealerTotal();

        if (dealerTotal > 21)
        {
            Outcome = BlackjackOutcome.PlayerWin;
            ResultMessage = "Dealer busted. You win.";
        }
        else if (playerTotal > dealerTotal)
        {
            Outcome = BlackjackOutcome.PlayerWin;
            ResultMessage = "You win.";
        }
        else if (playerTotal < dealerTotal)
        {
            Outcome = BlackjackOutcome.DealerWin;
            ResultMessage = "King Modi wins.";
        }
        else
        {
            Outcome = BlackjackOutcome.Push;
            ResultMessage = "Push.";
        }
    }

    public string RenderRoundState()
    {
        var output = new StringBuilder();
        output.Append("Modi: [");
        if (IsGameOver)
        {
            output.Append(string.Join(", ", dealerHand));
        }
        else
        {
            output.Append(dealerHand[0]);
            output.Append(", ?");
        }
        output.Append("]");

        output.AppendLine();
        output.Append("You: [");
        output.Append(string.Join(", ", playerHand));
        output.Append("] -> Total: ");
        output.Append(GetPlayerTotal());
        output.AppendLine();

        if (IsGameOver)
        {
            output.Append(ResultMessage);
        }
        else
        {
            output.Append("\"Hit or stand?\"");
        }

        return output.ToString();
    }

    public string RenderLossReason()
    {
        int playerTotal = GetPlayerTotal();
        if (playerTotal > 21)
        {
            return $"You busted!\n(Score: {playerTotal})";
        }

        return $"Modi was higher than you!\n(Modi: {GetDealerTotal()}\nScore: {playerTotal})";
    }

    public int GetPlayerTotal()
    {
        return CalculateHandTotal(playerHand);
    }

    public int GetDealerTotal()
    {
        return CalculateHandTotal(dealerHand);
    }

    private void BuildDeck()
    {
        for (int suit = 0; suit < 4; suit++)
        {
            foreach (string rank in CardRanks)
            {
                deck.Add(rank);
            }
        }
    }

    private void ShuffleDeck()
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int swapIndex = random.Next(i + 1);
            string currentCard = deck[i];
            deck[i] = deck[swapIndex];
            deck[swapIndex] = currentCard;
        }
    }

    private string DrawCard()
    {
        if (deck.Count == 0)
        {
            BuildDeck();
            ShuffleDeck();
        }

        int lastIndex = deck.Count - 1;
        string card = deck[lastIndex];
        deck.RemoveAt(lastIndex);
        return card;
    }

    private static int CalculateHandTotal(List<string> hand)
    {
        int total = 0;
        int aceCount = 0;

        foreach (string card in hand)
        {
            if (card == "A")
            {
                total += 11;
                aceCount++;
            }
            else if (card == "K" || card == "Q" || card == "J")
            {
                total += 10;
            }
            else
            {
                total += int.Parse(card);
            }
        }

        while (total > 21 && aceCount > 0)
        {
            total -= 10;
            aceCount--;
        }

        return total;
    }

    private void ResolveOpeningBlackjack()
    {
        bool playerBlackjack = GetPlayerTotal() == 21;
        bool dealerBlackjack = GetDealerTotal() == 21;

        if (!playerBlackjack && !dealerBlackjack) return;

        IsGameOver = true;

        if (playerBlackjack && dealerBlackjack)
        {
            Outcome = BlackjackOutcome.Push;
            ResultMessage = "Both have Blackjack. Push.";
        }
        else if (playerBlackjack)
        {
            Outcome = BlackjackOutcome.PlayerWin;
            ResultMessage = "Blackjack! You win.";
        }
        else
        {
            Outcome = BlackjackOutcome.DealerWin;
            ResultMessage = "King Modi has Blackjack. You lose.";
        }
    }
}
