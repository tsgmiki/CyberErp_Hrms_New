"use client";
import { useMemo } from "react";
import { getAllTrips } from "@/services/admin/trip";
import type { TripRequestModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import { money, tripStatusBadge } from "../trip/shared";
import { useTranslation } from "react-i18next";

interface Props {
  onSelect: (id: string) => void;
}

function MyTripList({ onSelect }: Props) {
  const { t } = useTranslation();
  const list = useEntityList({ queryKey: "myTrips", fetchPage: getAllTrips });

  const columns = useMemo(
    () =>
      [
        {
          name: "tripNumber",
          label: "Trip #",
          sort: true,
          render: (text: string, r: TripRequestModel) => (
            <button type="button" onClick={() => r.id && onSelect(r.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        { name: "tripType", label: "Type", render: (v: string) => t(v ?? "") },
        { name: "destination", label: "Destination" },
        { name: "dates", label: "Dates", render: (_t: unknown, r: TripRequestModel) => `${r.startDate?.slice(0, 10)} → ${r.endDate?.slice(0, 10)}` },
        { name: "advanceAmount", label: "Advance", render: (_t: unknown, r: TripRequestModel) => <span className="tabular-nums">{money(r.advanceAmount)}</span> },
        {
          name: "status",
          label: "Status",
          render: (text: string) => <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${tripStatusBadge(text)}`}>{t(text ?? "")}</span>,
        },
      ] as DataTableColumnModel[],
    [onSelect, t],
  );

  return <EntityListShell listKey="myTrips" listLabel="My Trips" columns={columns} {...list} />;
}

export default MyTripList;
