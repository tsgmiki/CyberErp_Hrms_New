/**
 * Calculate totals for order forms (Sales, Purchase, etc.)
 * This utility can be used across different order types
 */

export interface OrderDetailItem {
  quantity?: number;
  unitPrice?: number;
  unitCost?: number;
}

export interface OrderTaxDetail {
  amount?: number;
  rate?: number;
  isAddition?: boolean;
}

export interface OrderBankDetail {
  amount?: number;
}

export interface OrderTotals {
  subtotal: number;
  taxAmount: number;
  totalAmount: number;
  paidAmount: number;
  balanceDue: number;
  summaryString: string;
  taxDetails?: OrderTaxDetail[];
}

/**
 * Calculate order totals from details, taxDetails, and bankDetails
 * Also updates taxDetail amounts based on subtotal and rate
 * @param details - Array of order detail items with quantity and unitPrice
 * @param taxDetails - Array of tax detail items with rate
 * @param bankDetails - Array of bank/payment detail items with amount
 * @returns Object containing all calculated totals and updated taxDetails
 */
export function calculateOrderTotals(
  details?: OrderDetailItem[],
  taxDetails?: OrderTaxDetail[],
  bankDetails?: OrderBankDetail[],
): OrderTotals {
  // Calculate subtotal from details
  const subtotal =
    details?.reduce((sum, item) => {
      return (
        sum + (item.quantity || 0) * (item.unitPrice || item.unitCost || 0)
      );
    }, 0) || 0;

  // Update taxDetails with calculated amounts based on subtotal and rate
  const updatedTaxDetails =
    taxDetails?.map((tax) => ({
      ...tax,
      amount: subtotal * (tax.rate || 0) * (tax.isAddition ? 1 : -1),
    })) || [];

  // Calculate total tax amount from updated taxDetails
  const taxAmount =
    updatedTaxDetails?.reduce((sum, tax) => {
      return sum + (tax.amount || 0);
    }, 0) || 0;

  // Get bank/payment amount
  const paidAmount =
    bankDetails?.reduce((sum, bank) => {
      return sum + (bank.amount || 0);
    }, 0) || 0;

  const totalAmount = subtotal + taxAmount;
  const balanceDue = totalAmount - paidAmount;

  const summaryString =
    `Sub Total: ${Number(subtotal.toFixed(2)).toLocaleString()};` +
    `Tax: ${Number(taxAmount.toFixed(2)).toLocaleString()};` +
    `Total: ${Number(totalAmount.toFixed(2)).toLocaleString()};` +
    `Paid: ${Number(paidAmount.toFixed(2)).toLocaleString()};` +
    `Balance: ${Number(balanceDue.toFixed(2)).toLocaleString()}`;

  return {
    subtotal,
    taxAmount,
    totalAmount,
    paidAmount,
    balanceDue,
    summaryString,
    taxDetails: updatedTaxDetails,
  };
}
