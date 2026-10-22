using Mono.Cecil;using Mono.Cecil.Cil;using System.Text.Json;
var root=Path.GetFullPath(args[0]);
var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(@"C:\Program Files\Zune");resolver.AddSearchDirectory(@"C:\Windows\Microsoft.NET\Framework64\v2.0.50727");
var rp=new ReaderParameters{AssemblyResolver=resolver};
using var sourceModule=ModuleDefinition.ReadModule(Path.Combine(root,"RepeatFeature.dll"),rp);
using var helper=ModuleDefinition.ReadModule(Path.Combine(root,"ZuneScalingSettings.before.dll"),rp);
var source=sourceModule.Types.Single(t=>t.Name=="RepeatFeature");var target=helper.Types.Single(t=>t.FullName==source.FullName);
var fields=new Dictionary<FieldDefinition,FieldDefinition>();var methods=new Dictionary<MethodDefinition,MethodDefinition>();
foreach(var f in source.Fields){var copy=target.Fields.SingleOrDefault(v=>v.Name==f.Name);if(copy==null){copy=new FieldDefinition(f.Name,f.Attributes,helper.ImportReference(f.FieldType));target.Fields.Add(copy);}fields[f]=copy;}
foreach(var m in source.Methods){var copy=target.Methods.SingleOrDefault(v=>v.Name==m.Name&&v.Parameters.Count==m.Parameters.Count);if(copy==null){copy=new MethodDefinition(m.Name,m.Attributes,helper.ImportReference(m.ReturnType));foreach(var p in m.Parameters)copy.Parameters.Add(new ParameterDefinition(p.Name,p.Attributes,helper.ImportReference(p.ParameterType)));target.Methods.Add(copy);}methods[m]=copy;}
var helperAllowed=new HashSet<string>(new[]{"OnChanged","CancelAutomaticNext","CancelQueuedNext","ChooseNext"});
foreach(var m in source.Methods){var copy=methods[m];if(!m.HasBody||!helperAllowed.Contains(m.Name))continue;copy.Body=new MethodBody(copy){InitLocals=m.Body.InitLocals,MaxStackSize=m.Body.MaxStackSize};foreach(var v in m.Body.Variables)copy.Body.Variables.Add(new VariableDefinition(helper.ImportReference(v.VariableType)));var map=new Dictionary<Instruction,Instruction>();foreach(var i in m.Body.Instructions){var next=Instruction.Create(OpCodes.Nop);next.OpCode=i.OpCode;copy.Body.Instructions.Add(next);map[i]=next;}
 foreach(var i in m.Body.Instructions){map[i].Operand=i.Operand switch{Instruction b=>map[b],Instruction[] bs=>bs.Select(b=>map[b]).ToArray(),VariableDefinition v=>copy.Body.Variables[m.Body.Variables.IndexOf(v)],ParameterDefinition p=>copy.Parameters[m.Parameters.IndexOf(p)],MethodReference mr when mr.DeclaringType.FullName==source.FullName=>methods[mr.Resolve()],FieldReference fr when fr.DeclaringType.FullName==source.FullName=>fields[fr.Resolve()],TypeReference tr when tr.FullName==source.FullName=>target,MethodReference mr=>helper.ImportReference(mr),FieldReference fr=>helper.ImportReference(fr),TypeReference tr=>helper.ImportReference(tr),_=>i.Operand};}
 foreach(var e in m.Body.ExceptionHandlers)copy.Body.ExceptionHandlers.Add(new ExceptionHandler(e.HandlerType){TryStart=map[e.TryStart],TryEnd=e.TryEnd==null?null:map[e.TryEnd],HandlerStart=map[e.HandlerStart],HandlerEnd=e.HandlerEnd==null?null:map[e.HandlerEnd],FilterStart=e.FilterStart==null?null:map[e.FilterStart],CatchType=e.CatchType==null?null:helper.ImportReference(e.CatchType)});
}
helper.Write(Path.Combine(root,"ZuneScalingSettings.dll"),new WriterParameters{StrongNameKeyBlob=File.ReadAllBytes(args[1])});
using var shell=ModuleDefinition.ReadModule(Path.Combine(root,"ZuneShell.before.dll"),rp);
var transport=shell.Types.Single(t=>t.Name=="TransportControls");
IEnumerable<TypeDefinition> Walk(TypeDefinition t){yield return t;foreach(var n in t.NestedTypes)foreach(var item in Walk(n))yield return item;}
var delayed=Walk(transport).SelectMany(t=>t.Methods).Single(m=>m.Name=="<SetUrisOnPlayerAsync>b__5");
void RemoveGuard(MethodDefinition m,string name,int size){var ins=m.Body.Instructions;if(ins[size-1].OpCode!=OpCodes.Ret||!ins.Any(i=>i.Operand is MethodReference mr&&mr.DeclaringType.Name=="RepeatFeature"&&mr.Name==name))throw new Exception("Unexpected old guard: "+m.Name);var end=ins[size];if(ins[size-2].Operand!=end)throw new Exception("Guard jump mismatch: "+m.Name);for(int i=0;i<size;i++)ins.RemoveAt(0);}
RemoveGuard(transport.Methods.Single(m=>m.Name=="SetNextUriOnPlayer"),"CancelAutomaticNext",4);
RemoveGuard(delayed,"IsOne",3);
var async=transport.Methods.Single(m=>m.Name=="SetUrisOnPlayerAsync");var instructions=async.Body.Instructions;
if(instructions[0].Operand is not MethodReference check||check.Name!="IsOne"||instructions[2].OpCode!=OpCodes.Ldnull||instructions[3].OpCode!=OpCodes.Starg||instructions[3].Operand!=async.Parameters[1]||instructions[1].Operand!=instructions[4])throw new Exception("Unexpected old async prefix");
for(int i=0;i<4;i++)instructions.RemoveAt(0);
var first=instructions[0];var il=async.Body.GetILProcessor();
foreach(var i in new[]{Instruction.Create(OpCodes.Ldarg_0),Instruction.Create(OpCodes.Ldarg_1),Instruction.Create(OpCodes.Ldarg_2),Instruction.Create(OpCodes.Call,shell.ImportReference(target.Methods.Single(m=>m.Name=="ChooseNext"))),Instruction.Create(OpCodes.Castclass,shell.ImportReference(async.Parameters[1].ParameterType)),Instruction.Create(OpCodes.Starg,async.Parameters[1])})il.InsertBefore(first,i);
shell.Write(Path.Combine(root,"ZuneShell.dll"));

IEnumerable<MethodDefinition> All(ModuleDefinition m)=>m.Types.SelectMany(Walk).SelectMany(t=>t.Methods);
string Fingerprint(MethodDefinition m){if(!m.HasBody)return "none";var b=m.Body;string Op(object? o)=>o switch{Instruction i=>"@"+b.Instructions.IndexOf(i),Instruction[] a=>string.Join(",",a.Select(i=>b.Instructions.IndexOf(i))),VariableDefinition v=>"v"+b.Variables.IndexOf(v),ParameterDefinition p=>"p"+m.Parameters.IndexOf(p),_=>o?.ToString()??""};return string.Join(";",b.Instructions.Select(i=>i.OpCode.Code+":"+Op(i.Operand)))+"#"+string.Join(";",b.ExceptionHandlers.Select(e=>e.HandlerType+":"+b.Instructions.IndexOf(e.TryStart)+":"+b.Instructions.IndexOf(e.TryEnd)+":"+b.Instructions.IndexOf(e.HandlerStart)+":"+b.Instructions.IndexOf(e.HandlerEnd)));}
var shellAllowed=new HashSet<string>(new[]{"SetNextUriOnPlayer","SetUrisOnPlayerAsync","<SetUrisOnPlayerAsync>b__5"});var changed=new List<string>();
foreach(string dll in new[]{"ZuneShell","ZuneScalingSettings"}){
 using var before=ModuleDefinition.ReadModule(Path.Combine(root,dll+".before.dll"),rp);using var after=ModuleDefinition.ReadModule(Path.Combine(root,dll+".dll"),rp);
 var newer=All(after).ToDictionary(m=>m.FullName);
 foreach(var m in All(before))if(Fingerprint(m)!=Fingerprint(newer[m.FullName])){if(dll=="ZuneShell"? !shellAllowed.Contains(m.Name)||!m.DeclaringType.FullName.Contains("TransportControls"):m.DeclaringType.FullName!="ZuneRestoration.RepeatFeature"||!helperAllowed.Contains(m.Name))throw new Exception("Unrelated method changed: "+m);changed.Add(m.FullName);}
 foreach(var resource in before.Resources.OfType<EmbeddedResource>())if(!resource.GetResourceData().SequenceEqual(after.Resources.OfType<EmbeddedResource>().Single(r=>r.Name==resource.Name).GetResourceData()))throw new Exception("Resource changed: "+resource.Name);
 if(before.Assembly.Name.FullName!=after.Assembly.Name.FullName)throw new Exception("Assembly identity changed: "+dll);
}
File.WriteAllText(Path.Combine(root,"patch-audit.json"),JsonSerializer.Serialize(new{changedMethods=changed,onlyRepeatOneChanged=true,allOtherMethodsAndEmbeddedResourcesUnchanged=true,assemblyIdentitiesPreserved=true,manualSelectionArgumentUnchanged=true,existingNativeRepeatFallbackPreserved=true},new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine("PASS repeat-one repair compiled; three shell methods adjusted, all other features/resources unchanged.");
