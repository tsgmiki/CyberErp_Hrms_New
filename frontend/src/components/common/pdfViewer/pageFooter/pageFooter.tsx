"use client";

import { View, Text, StyleSheet } from "@react-pdf/renderer";

const styles = StyleSheet.create({
  footer: {
    position: "absolute",
    bottom: 0,
    left: 0,
    right: 0,
    flexDirection: "row",
    justifyContent: "center",
    alignItems: "center",
    padding: 8,
    fontSize: 10,
  },
  footerText: {
    textAlign: "center",
  },
});

function PageFooter() {
  return (
    <View style={styles.footer} fixed>
      <Text style={styles.footerText}>

      </Text>
    </View>
  );
}

export default PageFooter;
