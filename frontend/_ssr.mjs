import * as echarts from "echarts";
const hue = { BusinessUnit:"#2a78d6", Directorate:"#1baf7a", Division:"#eda100", Department:"#008300", Team:"#4a3aa7" };
const rgba=(h,a)=>{h=h.replace('#','');return `rgba(${parseInt(h.slice(0,2),16)},${parseInt(h.slice(2,4),16)},${parseInt(h.slice(4,6),16)},${a})`;};
const node=(name,type,hc,children)=>({name,rawType:type,headcount:hc,itemStyle:{color:rgba(hue[type],0.12),borderColor:hue[type],borderWidth:1.6},children});
const data=[node("Head Office","BusinessUnit",500,[
  node("Finance Directorate","Directorate",40,[ node("Accounts","Department",12,[]), node("Treasury","Department",8,[]) ]),
  node("Operations Directorate","Directorate",120,[ node("HR Division","Division",30,[ node("Recruitment","Team",5,[]), node("Payroll","Team",4,[]) ]) ]),
]) ];
const option={ backgroundColor:"#fff", series:[{ type:"tree", data, layout:"orthogonal", orient:"TB", edgeShape:"polyline", edgeForkPosition:"63%",
  initialTreeDepth:-1, symbol:"roundRect", symbolSize:[168,50], top:24,bottom:24,left:24,right:24, itemStyle:{borderRadius:8}, lineStyle:{color:"#d4d3cc",width:1.4},
  label:{ position:"inside", color:"#0b0b0b", formatter:(p)=> p.data.rawType?`{n|${p.data.name}}\n{t|${p.data.rawType}}`:`{h|${p.data.name}}`,
    rich:{ n:{fontSize:12.5,fontWeight:600,color:"#0b0b0b",align:"center",lineHeight:17}, t:{fontSize:9.5,color:"#52514e",align:"center"}, h:{fontSize:13,fontWeight:700,align:"center"} } },
  leaves:{label:{position:"inside"}} }] };
const chart=echarts.init(null,null,{renderer:"svg",ssr:true,width:1200,height:700});
chart.setOption(option);
const svg=chart.renderToSVGString();
import fs from "node:fs";
const OUT="C:/Users/tsgmi/AppData/Local/Temp/claude/D--Workspace-CyberErp-Hrms/414c1d51-bb34-4178-98b6-081ac27b15c1/scratchpad/orgchart.svg";
fs.writeFileSync(OUT, svg);
console.log("svg bytes:", svg.length);
console.log("node names present:", ["Head Office","Finance Directorate","Recruitment","Payroll","HR Division"].every(n=>svg.includes(n)));
console.log("type labels present:", ["BusinessUnit","Directorate","Division","Team","Department","Branch"].every(t=>svg.includes(t)));
console.log("polyline connectors:", (svg.match(/<polyline/g)||[]).length);
console.log("rounded node rects:", (svg.match(/<path/g)||[]).length > 0 || (svg.match(/<rect/g)||[]).length);
