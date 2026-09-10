"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useMemo, useState } from "react";
import type { AssignmentAudience, LearningAssignmentModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveLearningAssignment from "@/services/admin/learningAssignment/save";
import getLearningAssignment from "@/services/admin/learningAssignment/get";
import getAllTrainingCourse from "@/services/admin/trainingCourse/getAll";
import getAllOrganizationUnit from "@/services/admin/organizationUnit/getAll";
import getAllPositionClass from "@/services/admin/positionClass/getAll";
import getAllBranch from "@/services/admin/branch/getAll";
import Loading from "../../common/loader/loader";
import { activeStatusOptions, activeId, activeLabel } from "@/constants/orgStructure";
import { yesNoOptions, boolId, yesNoLabel } from "@/constants/leave";
import { parameterInitialData } from "@/constants/initialization";

const FormProvider = memo(FormProviders);

const NEW_DEFAULTS: LearningAssignmentModel = {
  isActive: true,
  audience: "Everyone",
  includeSubUnits: false,
  dueWithinDays: 30,
};

/** The audience list, in the order an author thinks about scope: broadest first. */
const AUDIENCES: { id: AssignmentAudience; name: string }[] = [
  { id: "Everyone", name: "Everyone" },
  { id: "OrganizationUnit", name: "Organizational Unit" },
  { id: "PositionClassAudience", name: "Job Role" },
  { id: "Branch", name: "Branch" },
];

/**
 * A mandatory-training rule.
 *
 * <p>The audience picker changes which target dropdown appears — one join from the employee in every
 * case, which is what keeps the nightly materialisation a set operation. Job grade is deliberately
 * not offered: grade is a pay concept here, and "everyone who does this job" is what a training
 * requirement actually means.</p>
 */
function LearningAssignmentForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<LearningAssignmentModel>({ ...NEW_DEFAULTS });
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["learningAssignment", id],
    queryFn: () => getLearningAssignment(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const [courseParam, setCourseParam] = useState({ ...parameterInitialData, take: 200 });
  const { data: courses, isLoading: coursesLoading } = useQuery({
    queryKey: ["trainingCourses", courseParam],
    queryFn: () => getAllTrainingCourse(courseParam),
  });

  const audience = (formData.audience ?? "Everyone") as AssignmentAudience;

  // Each target list is fetched only when its audience is selected — three dropdowns eagerly loaded
  // on every open would be three requests nobody asked for.
  const [unitParam, setUnitParam] = useState({ ...parameterInitialData, take: 300 });
  const { data: units, isLoading: unitsLoading } = useQuery({
    queryKey: ["organizationUnits", unitParam],
    queryFn: () => getAllOrganizationUnit(unitParam),
    enabled: audience === "OrganizationUnit",
  });

  const [classParam, setClassParam] = useState({ ...parameterInitialData, take: 300 });
  const { data: positionClasses, isLoading: classesLoading } = useQuery({
    queryKey: ["positionClasses", classParam],
    queryFn: () => getAllPositionClass(classParam),
    enabled: audience === "PositionClassAudience",
  });

  const [branchParam, setBranchParam] = useState({ ...parameterInitialData, take: 200 });
  const { data: branches, isLoading: branchesLoading } = useQuery({
    queryKey: ["branches", branchParam],
    queryFn: () => getAllBranch(branchParam),
    enabled: audience === "Branch",
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveLearningAssignment(fd);
    setFormState(result);
    setIsLoading(false);
  };

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);

  const selectHandler = useCallback((name: string, r: any) => {
    setFormData((p) => {
      const next = { ...p, [name]: r.id };
      // Switching audience clears the old target: an id left over from the previous kind points at
      // the wrong table and the server would reject it with a confusing message.
      if (name === "audience") { next.audienceId = undefined; next.audienceName = undefined; }
      return next;
    });
  }, []);

  useEffect(() => {
    if (typeof record != "undefined" && record != null) setFormData(record);
    else if (!id) setFormData({ ...NEW_DEFAULTS });
  }, [record, id]);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({ ...NEW_DEFAULTS });
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["learningAssignments"] });
      queryClient.invalidateQueries({ queryKey: ["learningAssignment"] });
      // The dashboard counts obligations this rule will produce on the next sweep.
      queryClient.invalidateQueries({ queryKey: ["complianceOverview"] });
      setId("");
    }
  }, [formState]);

  const audienceField = useMemo(() => {
    if (audience === "Everyone") return [];

    const config = {
      OrganizationUnit: {
        label: "Organizational Unit",
        data: units?.data?.map((u) => ({ id: u.id, name: u.name })),
        loading: unitsLoading, param: unitParam, setParam: setUnitParam,
      },
      PositionClassAudience: {
        label: "Job Role",
        data: positionClasses?.data?.map((p) => ({ id: p.id, name: p.title })),
        loading: classesLoading, param: classParam, setParam: setClassParam,
      },
      Branch: {
        label: "Branch",
        data: branches?.data?.map((b) => ({ id: b.id, name: b.name })),
        loading: branchesLoading, param: branchParam, setParam: setBranchParam,
      },
    }[audience];

    return [{
      name: "audienceId", label: config.label, placeholder: `Select ${config.label.toLowerCase()}`,
      required: true, type: "dropDown" as const, onSelect: selectHandler,
      value: formData.audienceId ?? undefined, displayValue: formData.audienceName ?? undefined,
      param: config.param, setParam: config.setParam as any, isLoading: config.loading,
      error: formState?.zodErrors?.audienceId,
      data: config.data as never,
    }];
  }, [audience, units, positionClasses, branches, unitsLoading, classesLoading, branchesLoading,
      unitParam, classParam, branchParam, formData.audienceId, formData.audienceName,
      selectHandler, formState]);

  return (
    <div className="text-white">
      {pending && <Loading />}
      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[30%]",
          isPending: isLoading,
          SubmitButton: "top",
          formId: "learningAssignmentForm",
          components: [
            { name: "name", label: "Name", placeholder: "e.g. Annual cold chain refresher", required: true, value: formData.name, onChange: changeHandler, error: formState?.zodErrors?.name, type: "text" },
            {
              name: "trainingCourseId", label: "Course", placeholder: "Select course", required: true,
              type: "dropDown", onSelect: selectHandler,
              value: formData.trainingCourseId, displayValue: formData.courseName ?? undefined,
              param: courseParam, setParam: setCourseParam as any, isLoading: coursesLoading,
              error: formState?.zodErrors?.trainingCourseId,
              data: courses?.data?.map((c) => ({ id: c.id, name: c.name })) as never,
            },
            {
              name: "audience", label: "Applies To", required: true, type: "dropDown",
              onSelect: selectHandler,
              value: audience,
              displayValue: AUDIENCES.find((a) => a.id === audience)?.name,
              data: AUDIENCES as never,
            },
            ...audienceField,
            ...(audience === "OrganizationUnit"
              ? [{
                  name: "includeSubUnits", label: "Include Sub-units", type: "dropDown" as const,
                  onSelect: selectHandler,
                  value: boolId(formData.includeSubUnits), displayValue: yesNoLabel(formData.includeSubUnits),
                  data: yesNoOptions as never,
                }]
              : []),
            { name: "dueWithinDays", label: "Due Within (days)", placeholder: "e.g. 30", required: true, value: formData.dueWithinDays, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "recurrenceMonths", label: "Repeats Every (months)", placeholder: "Blank = once only", value: formData.recurrenceMonths ?? undefined, onChange: changeHandler, inputType: "number", type: "text" },
            {
              name: "isActive", label: "Status", type: "dropDown", onSelect: selectHandler,
              value: activeId(formData.isActive), displayValue: activeLabel(formData.isActive),
              data: activeStatusOptions as never,
            },
            { name: "notes", label: "Notes", placeholder: "Why this is required", value: formData.notes ?? undefined, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

      {/* The two rules that most often surprise an author, said where they are being configured. */}
      <p className="mt-2 text-[11px] text-muted">
        Nobody is chased before they join or before this rule existed — a new assignment never lands
        on long-serving staff already overdue. A recurrence is measured from the day they completed
        it, not from a calendar date.
      </p>
    </div>
  );
}
export default LearningAssignmentForm;
