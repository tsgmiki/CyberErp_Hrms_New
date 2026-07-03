"use client";

import toCamelCase from "@/components/util/stringToCamelCase";
import type { TransactionApprovalModel } from "@/models";
import { View, Text, StyleSheet } from "@react-pdf/renderer";

function TransactionApproval(props: { approvals: TransactionApprovalModel[] }) {
  const { approvals } = props;
  const styles = StyleSheet.create({
    row: {
      flexDirection: "row",
      padding: 8,
    },
    column: {
      flex: 1,
    },
    text: {
      marginBottom: 0,
      fontSize: 10,
    },
    bold: {
      fontWeight: "bold",
    },
  });

  return (
    <View
      style={{
        flexDirection: "column",
        justifyContent: "space-between",
        margin: 0,
        padding: 16,
        fontSize: 10,
      }}
    >
      {/* Left Column */}
      {approvals
        ?.sort((a, b) =>
          new Date(a.date as string) > new Date(b.date as string) ? 1 : -1
        )
        .map((item,) => (
          <View style={styles.row}>
            {/* Left Column */}
            <View style={[styles.column, { alignItems: "flex-start" }]}>
              <Text style={styles.text}>
                <Text style={styles.bold}>
                  {toCamelCase(item.status as string) + " By"}{" "}
                </Text>
                {item.approver || item.user}
              </Text>
            </View>

            {/* Middle Column */}
            <View style={[styles.column, { alignItems: "center" }]}>
              <Text style={styles.text}>
                <Text style={styles.bold}>Date </Text>
                {new Date(item.date as string).toLocaleDateString()}
              </Text>
            </View>

            {/* Right Column */}
            <View style={[styles.column, { alignItems: "flex-end" }]}>
              <Text style={styles.text}>
                <Text style={styles.bold}>Signature </Text>
                .........................
              </Text>
            </View>
          </View>
        ))}
    </View>
  );
}

export default TransactionApproval;
