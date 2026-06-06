using System.Collections.Generic;
using Nucleus.Core.Model;

namespace Nucleus.Core.Command
{
    /// <summary>
    /// The shared live campaign the host owns and every mod renders a slice of: Commander (manual orders +
    /// mode), Build (buy menu), Squad (squads), Warfare (operations + feed + save/resume). Pure read models +
    /// commands (no Unity, no game refs) so it lives in the Campaign lib and crosses the host/mod boundary
    /// through <c>IModContext.Campaign</c>. Implemented by the Commander runtime's service and published once
    /// to the host; the other mods consume it.
    /// </summary>
    public interface ICampaign
    {
        // ---- read models ----
        IReadOnlyList<OrderState> Orders { get; }
        IReadOnlyList<UnitView> LastRoster { get; }
        HqSnapshot Hq();
        CommanderMode Mode();
        ConvoyCatalog Catalog();
        float Funds();
        AssignmentPreview PreviewAt(OrderKind kind, Vec3 world, DomainSet domains, float radius);

        // ---- commands ----
        OrderState PlaceOrder(OrderKind kind, Vec3 world, DomainSet domains, float radius);
        void ClearAll();
        void Clear(string orderId);
        void SetMode(CommanderMode mode);
        void ConfirmTopProposal();
        void ToggleSquadManual(string squadId);
        void ToggleOperationManual(string operationId);
        void BuyConvoy(string name);
        void SaveCampaign(string path);
        bool LoadCampaign(string path);
    }
}
