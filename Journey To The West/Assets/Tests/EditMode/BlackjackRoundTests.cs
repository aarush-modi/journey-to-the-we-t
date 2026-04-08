using NUnit.Framework;

[TestFixture]
public class BlackjackRoundTests
{
    private BlackjackRound round;

    [SetUp]
    public void SetUp()
    {
        round = new BlackjackRound();
    }

    [Test]
    public void NewRound_DealsCards()
    {
        Assert.Greater(round.GetPlayerTotal(), 0, "Player should have cards after construction");
        Assert.Greater(round.GetDealerTotal(), 0, "Dealer should have cards after construction");
    }

    [Test]
    public void Hit_AddsCardToPlayerHand()
    {
        // Due to randomness, opening blackjack may end the game immediately.
        // Retry with fresh rounds until we get one that is not over.
        BlackjackRound r = null;
        for (int attempt = 0; attempt < 100; attempt++)
        {
            r = new BlackjackRound();
            if (!r.IsGameOver) break;
        }

        Assert.IsFalse(r.IsGameOver, "Should find a round that is not immediately over");

        int totalBefore = r.GetPlayerTotal();
        r.Hit();

        // After a hit, the total should have changed (a card was added).
        // In extremely rare cases the added card could result in the same total
        // if aces are recalculated downward, so we verify the game progressed
        // by checking that either the total changed or the game is now over.
        bool totalChanged = r.GetPlayerTotal() != totalBefore;
        bool gameEnded = r.IsGameOver;
        Assert.IsTrue(totalChanged || gameEnded,
            "Hit should change the player total or end the game");
    }

    [Test]
    public void Hit_WhenGameOver_DoesNothing()
    {
        // Force game over by calling Stand
        BlackjackRound r = null;
        for (int attempt = 0; attempt < 100; attempt++)
        {
            r = new BlackjackRound();
            if (!r.IsGameOver) break;
        }

        if (!r.IsGameOver)
        {
            r.Stand();
        }

        Assert.IsTrue(r.IsGameOver);

        int totalBefore = r.GetPlayerTotal();
        BlackjackOutcome outcomeBefore = r.Outcome;

        r.Hit();

        Assert.AreEqual(totalBefore, r.GetPlayerTotal(),
            "Hit after game over should not change player total");
        Assert.AreEqual(outcomeBefore, r.Outcome,
            "Hit after game over should not change outcome");
    }

    [Test]
    public void Hit_PlayerBusts_OutcomeIsDealerWin()
    {
        // Keep hitting until the game ends from a bust or we exhaust attempts.
        bool foundBust = false;

        for (int attempt = 0; attempt < 200; attempt++)
        {
            var r = new BlackjackRound();
            if (r.IsGameOver) continue;

            // Hit repeatedly until bust or game over
            while (!r.IsGameOver)
            {
                r.Hit();
            }

            if (r.GetPlayerTotal() > 21)
            {
                Assert.AreEqual(BlackjackOutcome.DealerWin, r.Outcome,
                    "Player bust should result in DealerWin");
                Assert.IsTrue(r.IsGameOver);
                foundBust = true;
                break;
            }
        }

        Assert.IsTrue(foundBust, "Should have found at least one bust over many attempts");
    }

    [Test]
    public void Stand_DealerDrawsToSeventeenOrMore()
    {
        // Run several rounds to ensure the invariant holds
        for (int attempt = 0; attempt < 50; attempt++)
        {
            var r = new BlackjackRound();
            if (r.IsGameOver) continue; // opening blackjack

            r.Stand();

            Assert.GreaterOrEqual(r.GetDealerTotal(), 17,
                "After Stand, dealer total should be >= 17 (or bust)");
        }
    }

    [Test]
    public void Stand_PlayerHigher_PlayerWins()
    {
        bool foundCase = false;

        for (int attempt = 0; attempt < 500; attempt++)
        {
            var r = new BlackjackRound();
            if (r.IsGameOver) continue;

            r.Stand();

            if (r.GetPlayerTotal() > r.GetDealerTotal() && r.GetDealerTotal() <= 21)
            {
                Assert.AreEqual(BlackjackOutcome.PlayerWin, r.Outcome);
                foundCase = true;
                break;
            }
        }

        Assert.IsTrue(foundCase,
            "Should find at least one case where player total > dealer total (no bust)");
    }

    [Test]
    public void Stand_DealerHigher_DealerWins()
    {
        bool foundCase = false;

        for (int attempt = 0; attempt < 500; attempt++)
        {
            var r = new BlackjackRound();
            if (r.IsGameOver) continue;

            r.Stand();

            if (r.GetDealerTotal() > r.GetPlayerTotal() && r.GetDealerTotal() <= 21)
            {
                Assert.AreEqual(BlackjackOutcome.DealerWin, r.Outcome);
                foundCase = true;
                break;
            }
        }

        Assert.IsTrue(foundCase,
            "Should find at least one case where dealer total > player total (no bust)");
    }

    [Test]
    public void Stand_EqualTotals_IsPush()
    {
        bool foundCase = false;

        for (int attempt = 0; attempt < 1000; attempt++)
        {
            var r = new BlackjackRound();
            if (r.IsGameOver) continue;

            r.Stand();

            if (r.GetPlayerTotal() == r.GetDealerTotal())
            {
                Assert.AreEqual(BlackjackOutcome.Push, r.Outcome);
                foundCase = true;
                break;
            }
        }

        Assert.IsTrue(foundCase,
            "Should find at least one push case over many attempts");
    }

    [Test]
    public void Stand_DealerBusts_PlayerWins()
    {
        bool foundCase = false;

        for (int attempt = 0; attempt < 500; attempt++)
        {
            var r = new BlackjackRound();
            if (r.IsGameOver) continue;

            r.Stand();

            if (r.GetDealerTotal() > 21)
            {
                Assert.AreEqual(BlackjackOutcome.PlayerWin, r.Outcome);
                foundCase = true;
                break;
            }
        }

        Assert.IsTrue(foundCase,
            "Should find at least one case where dealer busts");
    }

    [Test]
    public void ResetRound_ClearsGameState()
    {
        // Play a round to completion
        if (!round.IsGameOver)
        {
            round.Stand();
        }

        Assert.IsTrue(round.IsGameOver, "Game should be over after Stand");

        round.ResetRound();

        // After reset, state should be fresh (unless opening blackjack occurs).
        // If opening blackjack happened, reset again until we get a clean state.
        for (int attempt = 0; attempt < 100; attempt++)
        {
            if (!round.IsGameOver) break;
            round.ResetRound();
        }

        Assert.IsFalse(round.IsGameOver, "IsGameOver should be false after reset (no opening blackjack)");
        Assert.AreEqual(BlackjackOutcome.None, round.Outcome, "Outcome should be None after reset");
        Assert.Greater(round.GetPlayerTotal(), 0, "Player should have cards after reset");
        Assert.Greater(round.GetDealerTotal(), 0, "Dealer should have cards after reset");
    }

    [Test]
    public void GetPlayerTotal_AceCountsCorrectly()
    {
        // The ace logic is tested indirectly: player total should always be <= 21
        // after the initial deal (with 2 cards, the max is 21 which is blackjack).
        // Even with two aces, the total should be 12 (11 + 1), not 22.
        for (int attempt = 0; attempt < 100; attempt++)
        {
            var r = new BlackjackRound();
            Assert.LessOrEqual(r.GetPlayerTotal(), 21,
                "Initial player total with 2 cards should never exceed 21 (aces adjust)");
            Assert.LessOrEqual(r.GetDealerTotal(), 21,
                "Initial dealer total with 2 cards should never exceed 21 (aces adjust)");
        }
    }

    [Test]
    public void RenderRoundState_ContainsPlayerTotal()
    {
        string state = round.RenderRoundState();

        Assert.IsTrue(state.Contains(round.GetPlayerTotal().ToString()),
            "Rendered state should contain the player total");
        Assert.IsTrue(state.Contains("You:"),
            "Rendered state should contain 'You:' label");
    }

    [Test]
    public void IsGameOver_FalseOnNewRound()
    {
        // Keep creating rounds until we get one without opening blackjack
        for (int attempt = 0; attempt < 100; attempt++)
        {
            var r = new BlackjackRound();
            if (!r.IsGameOver)
            {
                Assert.IsFalse(r.IsGameOver);
                return;
            }
        }

        Assert.Fail("Could not create a round without opening blackjack in 100 attempts");
    }

    [Test]
    public void Outcome_NoneOnNewRound()
    {
        // Keep creating rounds until we get one without opening blackjack
        for (int attempt = 0; attempt < 100; attempt++)
        {
            var r = new BlackjackRound();
            if (!r.IsGameOver)
            {
                Assert.AreEqual(BlackjackOutcome.None, r.Outcome);
                return;
            }
        }

        Assert.Fail("Could not create a round without opening blackjack in 100 attempts");
    }
}
