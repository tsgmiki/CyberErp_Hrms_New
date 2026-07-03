/**
 * Rewrites boilerplate getAll.ts → createPagedQuery (idempotent for the listed paths).
 *
 * WARNING: Overwrites files listed in `specs`. Do not run after hand-editing those
 * `getAll.ts` files unless you intend to replace them.
 *
 * Run: npm run migrate:getAll
 */
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");

/** [relativePath, apiPath, typeSource] — typeSource: model name from @/models or "record" */
const specs = [
  ["src/services/coffee/evaluationSubCategory/getAll.ts", "EvaluationSubCategory", "EvaluationSubCategoryModel"],
  ["src/services/coffee/evaluationCategory/getAll.ts", "EvaluationCategory", "EvaluationCategoryModel"],
  ["src/services/coffee/coding/getAll.ts", "Coding", "CodingModel"],
  ["src/services/coffee/sampling/getAll.ts", "Sampling", "SamplingModel"],
  ["src/services/coffee/evaluation/getAll.ts", "Evaluation", "EvaluationModel"],
  ["src/services/reports/finance/getAll.ts", "Report/Finance", "ReportModel"],
  ["src/services/inventories/storeUser/getAll.ts", "StoreUser", "StoreUserModel"],
  ["src/services/setting/lookup/getAll.ts", "Lookup", "LookupModel"],
  ["src/services/inventories/item/getAll.ts", "Item", "ItemModel"],
  ["src/services/setting/approver/getAll.ts", "Approver", "ApproverModel"],
  ["src/services/setting/documentSetting/getAll.ts", "DocumentSetting", "DocumentSettingModel"],
  ["src/services/setting/customer/getAll.ts", "Customer", "CustomerModel"],
  ["src/services/setting/notification/getAll.ts", "Notification", "NotificationModel"],
  ["src/services/inventories/store/getAll.ts", "Store", "StoreModel"],
  ["src/services/inventories/receive/getAll.ts", "Receive", "ReceiveModel"],
  ["src/services/inventories/receive/receiveBatchDetail/getAll.ts", "ReceiveBatchDetail", "ReceiveBatchDetailModel"],
  ["src/services/inventories/receive/receiveDetail/getAll.ts", "ReceiveDetail", "ReceiveDetailModel"],
  ["src/services/inventories/itemCategory/getAll.ts", "ItemCategory", "ItemCategoryModel"],
  ["src/services/admin/rolePermission/getAll.ts", "RolePermission", "RolePermissionModel"],
  ["src/services/inventories/inventoryOpening/getAll.ts", "InventoryOpening", "InventoryOpeningModel"],
  ["src/services/inventories/supplier/getAll.ts", "Supplier", "SupplierModel"],
  ["src/services/setting/workflow/getAll.ts", "Workflow", "WorkflowModel"],
  ["src/services/setting/fiscalYear/getAll.ts", "FiscalYear", "FiscalYearModel"],
  ["src/services/setting/taxType/getAll.ts", "TaxType", "TaxTypeModel"],
  ["src/services/inventories/itemConfig/getAll.ts", "ItemConfig", "ItemConfigModel"],
  ["src/services/inventories/purchaseOrderConfig/getAll.ts", "PurchaseOrderConfig", "PurchaseOrderConfigModel"],
  ["src/services/inventories/receiveConfig/getAll.ts", "ReceiveConfig", "ReceiveConfigModel"],
  ["src/services/inventories/transferConfig/getAll.ts", "TransferConfig", "TransferConfigModel"],
  ["src/services/inventories/adjustmentConfig/getAll.ts", "AdjustmentConfig", "AdjustmentConfigModel"],
  ["src/services/inventories/transfer/getAll.ts", "Transfer", "TransferModel"],
  ["src/services/finances/payment/getAll.ts", "Payment", "PaymentModel"],
  ["src/services/finances/collection/getAll.ts", "Collection", "CollectionModel"],
  ["src/services/finances/expense/getAll.ts", "Expense", "ExpenseModel"],
  ["src/services/finances/bankTransfer/getAll.ts", "BankTransfer", "BankTransferModel"],
  ["src/services/finances/expense/expenseType/getAll.ts", "ExpenseType", "record"],
  ["src/services/finances/accountTransaction/getAll.ts", "AccountTransaction", "AccountTransactionModel"],
  ["src/services/finances/paymentConfig/getAll.ts", "PaymentConfig", "PaymentConfigModel"],
  ["src/services/finances/collectionConfig/getAll.ts", "CollectionConfig", "CollectionConfigModel"],
  ["src/services/finances/bankTransferConfig/getAll.ts", "BankTransferConfig", "BankTransferConfigModel"],
  ["src/services/finances/expenseConfig/getAll.ts", "ExpenseConfig", "ExpenseConfigModel"],
  ["src/services/inventories/bincard/getAll.ts", "Bincard", "BincardModel"],
  ["src/services/inventories/inventory/getAll.ts", "Inventory", "InventoryModel"],
  ["src/services/sales/salesPerson/getAll.ts", "SalesPerson", "SalesPersonModel"],
  ["src/services/finances/bank/getAll.ts", "Bank", "BankModel"],
  ["src/services/inventories/batch/getAll.ts", "Batch", "BatchModel"],
  ["src/services/reportConfig/getAll.ts", "ReportConfig", "ReportConfigModel"],
  ["src/services/reports/sales/getAll.ts", "Report/Sales", "ReportModel"],
  ["src/services/reports/purchase/getAll.ts", "Report/Purchase", "ReportModel"],
  ["src/services/reports/inventory/getAll.ts", "Report/Inventory", "ReportModel"],
  ["src/services/reports/ledger/getAll.ts", "Report/Ledger", "ReportModel"],
  ["src/services/reports/summary/getAll.ts", "Report/Summary", "ReportModel"],
  ["src/services/setting/setting/getAll.ts", "Setting", "SettingModel"],
  ["src/services/sales/dailySummary/getAll.ts", "DailySummary", "DailySummaryModel"],
  ["src/services/inventories/transfer/transferDetail/getAll.ts", "TransferDetail", "TransferDetailModel"],
  ["src/services/sales/customer/getAll.ts", "Customer", "CustomerModel"],
  ["src/services/inventories/adjustment/getAll.ts", "Adjustment", "AdjustmentModel"],
  ["src/services/inventories/purchaseOrder/getAll.ts", "PurchaseOrder", "PurchaseOrderModel"],
  ["src/services/sales/delivery/getAll.ts", "Delivery", "DeliveryModel"],
  ["src/services/sales/deliveryConfig/getAll.ts", "DeliveryConfig", "DeliveryConfigModel"],
  ["src/services/sales/salesOrderConfig/getAll.ts", "SalesOrderConfig", "SalesOrderConfigModel"],
];

function render(relativePath, apiPath, typeName) {
  if (typeName === "record") {
    return `import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<Record<string, unknown>>("${apiPath}");
`;
  }
  return `import type { ${typeName} } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<${typeName}>("${apiPath}");
`;
}

let updated = 0;
for (const [rel, api, typeName] of specs) {
  const abs = path.join(root, rel);
  if (!fs.existsSync(abs)) {
    console.warn("skip missing", rel);
    continue;
  }
  const next = render(rel, api, typeName);
  fs.writeFileSync(abs, next, "utf8");
  updated += 1;
}
console.log("Updated", updated, "getAll.ts files");
