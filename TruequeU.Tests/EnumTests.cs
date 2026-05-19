using Microsoft.VisualStudio.TestTools.UnitTesting;
using TruequeU.Enums;

namespace TruequeU.Tests;

[TestClass]
public class ListingServiceTests
{
    [TestMethod]
    public void ListingState_ValuesShouldMatchExpected()
    {
        Assert.AreEqual(0, (int)ListingState.Available);
        Assert.AreEqual(1, (int)ListingState.Reserved);
        Assert.AreEqual(2, (int)ListingState.Sold);
        Assert.AreEqual(3, (int)ListingState.Disable);
    }

    [TestMethod]
    public void Category_ValuesShouldMatchExpected()
    {
        Assert.AreEqual(0, (int)Category.Books);
        Assert.AreEqual(1, (int)Category.Electronics);
        Assert.AreEqual(2, (int)Category.Furniture);
        Assert.AreEqual(3, (int)Category.Clothing);
        Assert.AreEqual(4, (int)Category.Other);
    }

    [TestMethod]
    public void ItemCondition_ValuesShouldMatchExpected()
    {
        Assert.AreEqual(0, (int)ItemCondition.New);
        Assert.AreEqual(1, (int)ItemCondition.LikeNew);
        Assert.AreEqual(2, (int)ItemCondition.UsedGood);
        Assert.AreEqual(3, (int)ItemCondition.UsedFair);
    }

    [TestMethod]
    public void ReportStatus_ValuesShouldMatchExpected()
    {
        Assert.AreEqual(0, (int)ReportStatus.Open);
        Assert.AreEqual(1, (int)ReportStatus.Closed);
    }

    [TestMethod]
    public void UserState_ValuesShouldMatchExpected()
    {
        Assert.AreEqual(0, (int)UserState.Active);
        Assert.AreEqual(1, (int)UserState.Suspended);
    }

    [TestMethod]
    public void ModerationActionType_ValuesShouldMatchExpected()
    {
        Assert.AreEqual(0, (int)ModerationActionType.HideListing);
        Assert.AreEqual(1, (int)ModerationActionType.UnhideListing);
        Assert.AreEqual(2, (int)ModerationActionType.SuspendUser);
        Assert.AreEqual(3, (int)ModerationActionType.UnsuspendUser);
    }
}
