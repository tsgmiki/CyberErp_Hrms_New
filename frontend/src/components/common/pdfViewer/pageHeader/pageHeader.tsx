"use client";

import { View, Text, StyleSheet, Image } from "@react-pdf/renderer";
import { useTranslation } from "react-i18next";
import logo from "@/assets/logo.png";

const styles = StyleSheet.create({
  page: {
    padding: 16,
    fontSize: 10,
  },
  header: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: 12,
    backgroundColor: "#f8fafc", // slate-50
    padding: 8,
  },
  logo: {
    width: 60,
    height: 60,
  },
  companyName: {
    fontSize: 16,
    fontWeight: "bold",
  },
});

function PageHeader() {
  const { t } = useTranslation();
  return (
    <View style={styles.header}>
      <Image source={logo} style={styles.logo} />
      <Text style={styles.companyName}>{t("CompanyName")}</Text>
    </View>
  );
}

export default PageHeader;
