namespace InvenFlow.Api.Dtos;

public record CreatePurchaseDto(
    int SupplierId,
    int WarehouseId,
    bool IsCash,
    string Note,
    DateTime? Date,
    List<PurchaseItemDto> Items);
public record PurchaseItemDto(int ProductId, int Qty, decimal UnitCost);

public record CreateSaleDto(
    int CustomerId,
    int WarehouseId,
    bool IsCash,
    string Note,
    DateTime? Date,
    List<SaleItemDto> Items);
public record SaleItemDto(int ProductId, int Qty, decimal UnitPrice);

public record CreatePaymentDto(int SupplierId, decimal Amount, string Method, int? PurchaseId, DateTime? Date, string Note);
public record CreateReceiptDto(int CustomerId, decimal Amount, string Method, int? SaleId, DateTime? Date, string Note);

public record CreateAdjustmentDto(int ProductId, int WarehouseId, int Delta, string Note);
