import type { FormComponentModel } from "@/models";
import { ZodErrors } from "../common/statusMessage/status";

import { useEditor } from "@tiptap/react";
import StarterKit from "@tiptap/starter-kit";
import { EditorContent } from "@tiptap/react";
import Heading from "@tiptap/extension-heading";
import Image from "@tiptap/extension-image";
import { useEffect } from "react";
import { useTranslation } from "react-i18next";
import Highlight from "@tiptap/extension-highlight";
import TextAlign from "@tiptap/extension-text-align";
import {
  Bold,
  Italic,
  Strikethrough,
  Heading1,
  Heading2,
  Heading3,
  List,
  ListOrdered,
  AlignLeft,
  AlignRight,
  AlignCenter,
  AlignJustify,
} from "lucide-react";
import Tooltip from "../common/toolTip/tooltip";

const HtmlEditorField = ({
  error,
  required = defaultProps.required,
  name,
  label,
  labelWidth,
  value,
  colSpan,
  onHtmlChange,
}: FormComponentModel) => {
  const { t } = useTranslation();

  // Unified theme classes
  const labelClass = "text-foreground";
  const toolbarClass = "bg-background border border-border";
  const toolbarButtonActiveClass = "bg-primary text-on-accent";
  const toolbarButtonInactiveClass = "text-foreground hover:bg-secondary";
  const toolbarSeparatorClass = "bg-border";
  const editorContainerClass = "bg-card border border-border text-foreground";
  const editorFocusClass = "focus-within:border-primary focus-within:ring-2 focus-within:ring-primary/20";

  const editor = useEditor({
    extensions: [
      StarterKit,
      Heading,
      Image,
      TextAlign.configure({
        types: ["heading", "paragraph"],
      }),
      Highlight,
    ],
    content: value || "",

    editable: true,
    onUpdate({ editor }) {
      onHtmlChange?.(editor?.getHTML());
    },
  });
  useEffect(() => {
    if (value != null && editor) {
      if (editor.getHTML() !== value) {
        editor.commands.setContent(value);
      }
    }
  }, [value, editor]);

  const toolbarGroups = [
    {
      items: [
        {
          label: "Bold",
          icon: Bold,
          action: () => editor?.chain().focus().toggleBold().run(),
          isActive: () => editor?.isActive("bold"),
        },
        {
          label: "Italic",
          icon: Italic,
          action: () => editor?.chain().focus().toggleItalic().run(),
          isActive: () => editor?.isActive("italic"),
        },
        {
          label: "Strike",
          icon: Strikethrough,
          action: () => editor?.chain().focus().toggleStrike().run(),
          isActive: () => editor?.isActive("strike"),
        },
      ],
    },
    {
      items: [
        {
          label: "Heading 1",
          icon: Heading1,
          action: () =>
            editor?.chain().focus().toggleHeading({ level: 1 }).run(),
          isActive: () => editor?.isActive("heading", { level: 1 }),
        },
        {
          label: "Heading 2",
          icon: Heading2,
          action: () =>
            editor?.chain().focus().toggleHeading({ level: 2 }).run(),
          isActive: () => editor?.isActive("heading", { level: 2 }),
        },
        {
          label: "Heading 3",
          icon: Heading3,
          action: () =>
            editor?.chain().focus().toggleHeading({ level: 3 }).run(),
          isActive: () => editor?.isActive("heading", { level: 3 }),
        },
      ],
    },
    {
      items: [
        {
          label: "Bullet List",
          icon: List,
          action: () => editor?.chain().focus().toggleBulletList().run(),
          isActive: () => editor?.isActive("bulletList"),
        },
        {
          label: "Ordered List",
          icon: ListOrdered,
          action: () => editor?.chain().focus().toggleOrderedList().run(),
          isActive: () => editor?.isActive("orderedList"),
        },
      ],
    },
    {
      items: [
        {
          label: "Align Left",
          icon: AlignLeft,
          action: () => editor?.chain().focus().setTextAlign("left").run(),
          isActive: () => editor?.isActive({ textAlign: "left" }),
        },
        {
          label: "Align Center",
          icon: AlignCenter,
          action: () => editor?.chain().focus().setTextAlign("center").run(),
          isActive: () => editor?.isActive({ textAlign: "center" }),
        },
        {
          label: "Align Right",
          icon: AlignRight,
          action: () => editor?.chain().focus().setTextAlign("right").run(),
          isActive: () => editor?.isActive({ textAlign: "right" }),
        },
        {
          label: "Justify",
          icon: AlignJustify,
          action: () => editor?.chain().focus().setTextAlign("justify").run(),
          isActive: () => editor?.isActive({ textAlign: "justify" }),
        },
      ],
    },
  ];

  return (
    <div
      key={name}
      className={`${colSpan != "full" && "md:inline-flex"} gap-1 w-full`}
    >
      {" "}
      <label
        className={`${labelClass} font-semibold text-sm col-span-1 max-md:w-full text-end flex items-center justify-end gap-0.5 ${
          colSpan == "full"
            ? "w-full"
            : typeof labelWidth != "undefined"
              ? labelWidth
              : "w-[20%]"
        }`}
      >
        {label ? t(label) : ""}
        <span className={required ? "text-error text-lg pl-2 align-middle" : "text-transparent text-lg pl-2 align-middle"}>*</span>
      </label>
      <div className={"w-full gap"}>
        <div
          className={`toolbar flex flex-wrap items-center gap-1 mb-2 p-1.5 backdrop-blur-sm rounded-lg border sticky top-0 z-10 ${toolbarClass}`}
        >
          {toolbarGroups.map((group, groupIndex) => (
            <div key={groupIndex} className="flex items-center gap-0.5">
              {groupIndex > 0 && (
                <div className={`w-px h-5 mx-1.5 ${toolbarSeparatorClass}`} />
              )}
              {group.items.map((item, itemIndex) => (
                <Tooltip key={itemIndex} message={item.label}>
                  <button
                    type="button"
                    onClick={(e) => {
                      e.stopPropagation();
                      item.action();
                    }}
                    className={`p-1.5 rounded-md transition-all duration-200 ${
                      item.isActive()
                        ? toolbarButtonActiveClass
                        : toolbarButtonInactiveClass
                    }`}
                  >
                    <item.icon
                      size={18}
                      strokeWidth={2.5}
                      className="w-4 h-4"
                    />
                  </button>
                </Tooltip>
              ))}
            </div>
          ))}
        </div>
        <div
          className={`prose prose-sm max-w-none border rounded-lg p-4 min-h-75 focus-within:ring-2 transition-all duration-200 ${editorContainerClass} ${editorFocusClass}`}
          onClick={() => editor?.commands.focus()}
        >
          <EditorContent
            className="focus:outline-none min-h-65"
            editor={editor}
          />
        </div>

        <span className={"pb-1 block flex-none"}>
          <ZodErrors error={error} />
        </span>
      </div>
    </div>
  );
};
const defaultProps = {
  value: "",
  disabled: false,
  required: false,
  inputType: "text",
  className: "",
};

export default HtmlEditorField;
