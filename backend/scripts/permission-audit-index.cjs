const fs=require("fs"), path=require("path");
const root="CyberErp.Hrms.App/Features";
const files=[];
(function walk(d){for(const e of fs.readdirSync(d,{withFileTypes:true})){
  const p=path.join(d,e.name);
  if(e.isDirectory()) walk(p); else if(e.name.endsWith(".cs")) files.push(p);}})(root);

// class name -> {file, body}
const classes={};
for(const f of files){
  const src=fs.readFileSync(f,"utf8").replace(/\r\n/g,"\n");
  const re=/\n    public (?:sealed )?class (\w+)\s*\(/g;   // primary-constructor handlers
  let m, marks=[];
  while((m=re.exec(src))) marks.push({name:m[1], start:m.index});
  for(let i=0;i<marks.length;i++){
    const end = i+1<marks.length ? marks[i+1].start : src.length;
    classes[marks[i].name]={file:f.split("/").pop(), body:src.slice(marks[i].start,end)};
  }
}
fs.writeFileSync(process.argv[2]+"/classes.json", JSON.stringify(classes));
console.log("indexed "+Object.keys(classes).length+" handler classes");
