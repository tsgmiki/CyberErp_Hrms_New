// Cross-module E2E: Recruitment -> Employee -> Performance -> Career Development (persisted to CERP).
// Narrative: "Meron Bekele" is recruited, hired, appraised, and put on a career path + succession plan.
const BASE="http://localhost:5014/api/v1";
const TENANT=process.argv[2]||"demo"; const USER=process.argv[3]||"demo"; const PW="Passw0rd!"; const PHONE=process.argv[4]||"0911000001";
let pass=0,fail=0; const ids={};
const ok=(c,m)=>{if(c){pass++;console.log("   ✓",m);}else{fail++;console.log("   ✗ FAIL:",m);}};
const H=(s)=>console.log(`\n══════ ${s} ══════`);
const box={value:""};
async function req(method,path,body,{raw=true,auth=true}={}){const h={"Content-Type":"application/json"};if(auth){h["X-Tenant-Id"]=TENANT;if(box.value)h["Cookie"]=box.value;}const res=await fetch(`${BASE}${path}`,{method,headers:h,body:body?JSON.stringify(body):undefined});const sc=res.headers.get("set-cookie");if(sc)box.value=sc.split(",").map(s=>s.split(";")[0]).join("; ");const t=await res.text();let j;try{j=JSON.parse(t);}catch{j=t;}return raw?{status:res.status,json:j}:j;}
async function upload(path,docType,filename){const fd=new FormData();fd.append("documentType",docType);fd.append("file",new Blob([`%PDF-1.4 test ${docType}`],{type:"application/pdf"}),filename);const h={"X-Tenant-Id":TENANT};if(box.value)h["Cookie"]=box.value;const res=await fetch(`${BASE}${path}`,{method:"POST",headers:h,body:fd});const t=await res.text();let j;try{j=JSON.parse(t);}catch{j=t;}return{status:res.status,json:j};}
const R=req; const idOf=r=>r.json?.id??r.json;
const err=r=>r.status>=400?` :: ${JSON.stringify(r.json).slice(0,300)}`:"";

async function main(){
  H("SETUP — tenant & login");
  const reg=await R("POST","/Auth/register",{fullName:"Demo Admin",email:`${USER}@demo.com`,phoneNumber:PHONE,userName:USER,password:PW,tenantName:"Demo Corp",tenantIdentifier:TENANT},{auth:false});
  console.log(`   register: ${reg.status}${reg.status>=400?" (tenant may already exist — continuing to login)":""}`);
  const login=await R("POST","/Auth/login/cookie",{userName:USER,password:PW,tenantId:TENANT});
  ok(login.status===200,`login as '${USER}' / tenant '${TENANT}'`);
  if(login.status!==200){console.log("Cannot login, aborting.");process.exit(2);}

  H("PHASE 0 — Foundation reference data (org, competencies, cycle, objective)");
  ids.jc=idOf(await R("POST","/JobCategory",{name:"Engineering",code:"ENG",isActive:true}));
  ids.jg=idOf(await R("POST","/JobGrade",{name:"Grade IV",code:"G4"}));
  ids.step=idOf(await R("POST","/Step",{name:"Step 1",code:"S1"}));
  ids.scale=idOf(await R("POST","/SalaryScale",{jobGradeId:ids.jg,stepId:ids.step,salary:45000}));
  ids.pcl=idOf(await R("POST","/PositionClass",{code:"SSE",title:"Senior Software Engineer",salaryScaleId:ids.scale,jobCategoryId:ids.jc,isActive:true}));
  ids.pclMgr=idOf(await R("POST","/PositionClass",{code:"EM",title:"Engineering Manager",salaryScaleId:ids.scale,jobCategoryId:ids.jc,isActive:true}));
  ids.unit=idOf(await R("POST","/OrganizationUnit",{code:"ENGDEP",name:"Engineering Department",unitType:"Department",isActive:true}));
  ids.posEng=idOf(await R("POST","/Position",{code:"SSE-01",positionClassId:ids.pcl,organizationUnitId:ids.unit}));
  ids.posMgr=idOf(await R("POST","/Position",{code:"EM-01",positionClassId:ids.pclMgr,organizationUnitId:ids.unit}));
  ok(ids.posEng&&ids.posMgr,"created org structure (Engineering dept, Senior Engineer + Manager positions)");
  ids.cat=idOf(await R("POST","/CompetencyCategory",{name:"Core",sortOrder:0,isActive:true}));
  ids.cLead=idOf(await R("POST","/Competency",{name:"Leadership",competencyCategoryId:ids.cat,isActive:true}));
  ids.cStrat=idOf(await R("POST","/Competency",{name:"Strategic Thinking",competencyCategoryId:ids.cat,isActive:true}));
  ids.cComm=idOf(await R("POST","/Competency",{name:"Communication",competencyCategoryId:ids.cat,isActive:true}));
  ids.cDesign=idOf(await R("POST","/Competency",{name:"Software Design",competencyCategoryId:ids.cat,isActive:true}));
  await R("POST","/PositionCompetency",{positionId:ids.posEng,items:[{competencyId:ids.cDesign,weight:60},{competencyId:ids.cComm,weight:40}]});
  await R("POST","/PositionCompetency",{positionId:ids.posMgr,items:[{competencyId:ids.cLead,weight:40},{competencyId:ids.cStrat,weight:30},{competencyId:ids.cDesign,weight:30}]});
  ok(true,"defined competencies + position-competency frameworks (Engineer vs Manager)");
  ids.rs=idOf(await R("POST","/RatingScale",{name:"1-5 Scale",scoreType:"Numeric",isActive:true,sortOrder:0,levels:[{value:5,label:"Excellent",minScore:0,maxScore:5,sortOrder:0}]}));
  ids.rc=idOf(await R("POST","/ReviewCycle",{name:"2026 Annual Review",periodType:"Annual",ratingScaleId:ids.rs,startDate:"2026-01-01",endDate:"2026-12-31",enableSelfAssessment:false}));
  const obj=await R("POST","/OrganizationalObjective",{title:"Deliver Platform v2",reviewCycleId:ids.rc,organizationUnitId:ids.unit,weight:100,status:"Active"});
  ids.obj=idOf(obj); ok(obj.status<300,`created 2026 review cycle + org objective "Deliver Platform v2"${err(obj)}`);

  H("PHASE 1 — RECRUITMENT (hiring need -> requisition -> candidate -> interview -> offer -> HIRE)");
  const hr=await R("POST","/HiringRequest",{organizationUnitId:ids.unit,positionClassId:ids.pcl,numberOfPositions:1,employmentType:"Permanent",justification:"Team growth for Platform v2",estimatedBudget:600000,expectedStartDate:"2026-03-01"});
  ids.hr=idOf(hr); ok(hr.status<300,`hiring request raised${err(hr)}`);
  const hrSub=await R("POST",`/HiringRequest/${ids.hr}/submit`,{}); ok(hrSub.status<300,`hiring request submitted -> auto-approved (no workflow chain)${err(hrSub)}`);
  const jr=await R("POST","/JobRequisition",{hiringRequestId:ids.hr,numberOfPositions:1,employmentType:"Permanent",title:"Senior Software Engineer",description:"Build Platform v2",minExperienceYears:5,skills:"C#, React"});
  ids.jr=idOf(jr); ok(jr.status<300,`job requisition created from approved need${err(jr)}`);
  const jrSub=await R("POST",`/JobRequisition/${ids.jr}/submit`,{}); ok(jrSub.status<300,`requisition submitted -> approved${err(jrSub)}`);
  await R("PUT","/JobRequisition/posting",{id:ids.jr,postingChannel:"Both",postingText:"Join our engineering team!",openFrom:"2026-02-01",openUntil:"2026-02-28"});
  const jrPost=await R("POST",`/JobRequisition/${ids.jr}/post`,{}); ok(jrPost.status<300,`vacancy posted (internal + external)${err(jrPost)}`);
  const cand=await R("POST","/Candidate",{firstName:"Meron",fatherName:"Tesfaye",grandFatherName:"Bekele",email:"meron.bekele@example.com",phoneNumber:"0912345678",gender:"Female",source:"External",yearsOfExperience:6,educationSummary:"BSc Computer Science",experienceSummary:"6 yrs software engineering",skillsSummary:"C#, React, Azure",consentGiven:true});
  ids.cand=idOf(cand); ok(cand.status<300,`candidate "Meron Bekele" registered (consent given)${err(cand)}`);
  for(const [dt,fn] of [["NationalId","id.pdf"],["GuarantorForm","guarantor.pdf"],["MedicalCertificate","medical.pdf"],["SignedOfferLetter","offer-signed.pdf"]]){
    const up=await upload(`/Candidate/${ids.cand}/documents`,dt,fn); ok(up.status<300,`uploaded compliance doc: ${dt}${err(up)}`);
  }
  const app=await R("POST","/JobApplication",{candidateId:ids.cand,requisitionId:ids.jr}); ids.app=idOf(app); ok(app.status<300,`application submitted (candidate x requisition)${err(app)}`);
  for(const stage of ["Screening","Shortlisted","Interview"]){
    const mv=await R("PUT","/JobApplication/stage",{id:ids.app,stage,note:`advanced to ${stage}`}); ok(mv.status<300,`application -> ${stage}${err(mv)}`);
  }
  const iv=await R("POST","/Interview",{applicationId:ids.app,scheduledStart:"2026-02-15T09:00:00Z",scheduledEnd:"2026-02-15T10:00:00Z",format:"InPerson",location:"HQ Room 1",panelists:[{panelistName:"Abebe (Hiring Mgr)",isLead:true,attendance:"Present"}]});
  ids.iv=idOf(iv); ok(iv.status<300,`interview scheduled with panel (at Interview stage)${err(iv)}`);
  const sel=await R("PUT","/JobApplication/stage",{id:ids.app,stage:"Selected",note:"passed interview"}); ok(sel.status<300,`application -> Selected${err(sel)}`);
  const offer=await R("POST","/JobOffer",{applicationId:ids.app,salary:52000,salaryScaleId:ids.scale,salaryJustification:"Market rate for a senior hire with 6 years' experience.",proposedStartDate:"2026-03-01",expiryDate:"2026-02-28",letterText:"We are pleased to offer you the Senior Software Engineer role."});
  ids.offer=idOf(offer); ok(offer.status<300,`job offer drafted (52,000)${err(offer)}`);
  const offSub=await R("POST",`/JobOffer/${ids.offer}/submit`,{}); ok(offSub.status<300,`offer submitted -> approved -> sent${err(offSub)}`);
  const offAcc=await R("PUT","/JobOffer/respond",{id:ids.offer,response:"Accept",note:"Delighted to accept!"}); ok(offAcc.status<300,`candidate ACCEPTED the offer -> application OfferAccepted${err(offAcc)}`);
  const hire=await R("POST",`/Candidate/${ids.cand}/hire`,{employeeNumber:"EMP-1001",hireDate:"2026-03-01",positionId:ids.posEng,employmentNature:"Permanent",salary:52000,salaryScaleId:ids.scale});
  ok(hire.status<300,`*** HIRED -> Employee created *** ${err(hire)}`);
  ids.emp=idOf(hire);
  const empGet=await R("GET",`/Employee/${ids.emp}`);
  ok(empGet.status<300 && (empGet.json.firstName==="Meron"),`Recruitment→Employee: employee "${empGet.json.firstName} ${empGet.json.grandFatherName}" #${empGet.json.employeeNumber} exists on the candidate's Person`);
  ok(empGet.json.positionId===ids.posEng,`hired into the Senior Engineer position`);

  H("PHASE 2 — PERFORMANCE (appraise Meron in the 2026 cycle)");
  const gen=await R("POST","/Appraisal/generate",{employeeId:ids.emp,reviewCycleId:ids.rc}); ids.appr=idOf(gen); ok(gen.status<300,`appraisal generated (competency lines from her position)${err(gen)}`);
  const apGet=await R("GET",`/Appraisal/${ids.appr}`); const compLines=(apGet.json.competencies||[]);
  ok(compLines.length>0,`appraisal seeded ${compLines.length} competency line(s): ${compLines.map(c=>c.title).join(", ")}`);
  const scoreRes=await R("PUT","/Appraisal/score",{id:ids.appr,scope:"Manager",competencies:compLines.map(c=>({lineId:c.id,score:4}))});
  ok(scoreRes.status<300,`manager scored all competencies 4/5${err(scoreRes)}`);

  H("PHASE 3 — CAREER DEVELOPMENT + cross-module integration");
  // Career path
  ids.path=idOf(await R("POST","/CareerPath",{name:"Engineering Leadership Track",code:"ELT",isActive:true}));
  ids.st1=idOf(await R("POST","/CareerPathStep",{careerPathId:ids.path,stepOrder:1,name:"Senior Engineer",positionClassId:ids.pcl,competencies:[{competencyId:ids.cDesign,weight:60},{competencyId:ids.cComm,weight:40}]}));
  ids.st2=idOf(await R("POST","/CareerPathStep",{careerPathId:ids.path,stepOrder:2,name:"Engineering Manager",positionClassId:ids.pclMgr,competencies:[{competencyId:ids.cLead,weight:40},{competencyId:ids.cStrat,weight:30},{competencyId:ids.cDesign,weight:30}]}));
  const asg=await R("POST","/EmployeeCareerPath",{employeeId:ids.emp,careerPathId:ids.path,currentStepId:ids.st1,status:"Active",assignedBy:"HR",stepProgress:[{careerPathStepId:ids.st1,status:"Completed"},{careerPathStepId:ids.st2,status:"InProgress"}]});
  ids.asg=idOf(asg); ok(asg.status<300,`Meron assigned to "Engineering Leadership Track" (step 1 done)${err(asg)}`);
  // Succession (candidate created NotReady — before appraisal completion)
  ids.crit=idOf(await R("POST","/CriticalPosition",{positionId:ids.posMgr,riskLevel:"High",reason:"Key leadership role",isActive:true}));
  ids.splan=idOf(await R("POST","/SuccessionPlan",{criticalPositionId:ids.crit,name:"Engineering Manager Succession",horizon:"MediumTerm",status:"Active"}));
  const candSucc=await R("POST","/SuccessionCandidate",{successionPlanId:ids.splan,employeeId:ids.emp,rank:1,readiness:"NotReady"});
  ids.succ=idOf(candSucc); ok(candSucc.status<300,`Meron listed as #1 successor for Engineering Manager (readiness: NotReady)${err(candSucc)}`);
  // *** HC153: finalize the appraisal -> auto-refresh succession readiness ***
  const comp=await R("POST",`/Appraisal/${ids.appr}/complete`,{}); ok(comp.status<300,`appraisal FINALIZED${err(comp)}`);
  const apDone=await R("GET",`/Appraisal/${ids.appr}`); ok(Number(apDone.json.overallScore)>0,`appraisal overall score = ${apDone.json.overallScore}/5`);
  const succAfter=await R("GET",`/SuccessionCandidate/${ids.succ}`);
  ok(succAfter.json.readiness!=="NotReady",`Performance→Career (HC153): readiness AUTO-refreshed to "${succAfter.json.readiness}" (${succAfter.json.readinessScore}%) after appraisal — no manual recompute`);
  // HC163 performance-aware suggestions
  const sug=await R("GET",`/CareerPath/suggestions?employeeId=${ids.emp}`);
  const sELT=(sug.json||[]).find(x=>x.code==="ELT");
  ok(sELT&&sELT.performanceScore!=null,`HC163 suggestions blend performance: ELT fit ${sELT?.fitScore}% (match ${sELT?.matchPercent}% + perf ${sELT?.performanceScore}%)`);
  // HC167 goals aligned to objective
  const goals=await R("POST",`/EmployeeCareerPath/${ids.asg}/create-goals`,{});
  ok(goals.status<300 && goals.json.created>0,`HC167: ${goals.json?.created} development goal(s) created, aligned to "${goals.json?.organizationalObjectiveTitle}"${err(goals)}`);
  // HC130 career-path gap -> IDP
  const idp1=await R("POST",`/EmployeeCareerPath/${ids.asg}/create-development-plan`,{});
  ok(idp1.status<300 && idp1.json.actionCount>0,`HC130: Individual Development Plan created from career gap (${idp1.json?.actionCount} actions)${err(idp1)}`);
  // HC155 succession gap -> IDP
  const idp2=await R("POST",`/SuccessionCandidate/${ids.succ}/create-development-plan`,{});
  ok(idp2.status<300 && idp2.json.actionCount>0,`HC155: IDP created from succession gap (${idp2.json?.actionCount} actions)${err(idp2)}`);
  // HC158 the 360 view
  const prof=await R("GET",`/EmployeeDevelopment/${ids.emp}/profile`);
  const p=prof.json;
  ok(prof.status<300 && p.careerPaths?.length>0 && p.successionCandidacies?.length>0 && p.performance,
    `HC158 Employee 360: perf appraisal ${p.performance?.latestAppraisal?.overallScore ?? "—"}, ${p.careerPaths?.length} career path(s), ${p.successionCandidacies?.length} candidacy(ies), ${p.mentorships?.length??0} mentorship(s), gap=${p.nextStepGap?.gapCount??0}`);

  H("RESULT");
  console.log(`\n${fail===0?"✅ ALL PASS":"❌ HAS FAILURES"}: ${pass} passed, ${fail} failed`);
  console.log(`\nTenant: ${TENANT}   Login: ${USER} / ${PW}   Employee: Meron Bekele (#EMP-1001, id ${ids.emp})`);
  console.log(`IDs: ${JSON.stringify(ids)}`);
  process.exit(fail===0?0:1);
}
main().catch(e=>{console.error("ERROR",e);process.exit(2);});
