using System;
using Lamar.IoC.Instances;
using LamarCompiler.Frames;
using LamarCompiler.Scenarios;
using Shouldly;
using Xunit;

namespace LamarCompiler.Testing.Codegen
{
    public class ConstructorFrameTests
    {
        public interface IGuy{}
        
        public class NoArgGuy : IGuy, IDisposable
        {
            public NoArgGuy()
            {
            }

            public void Dispose()
            {
                WasDisposed = true;
            }

            public bool WasDisposed { get; set; }
            
            public int Number { get; set; }
            public double Double { get; set; }
            public string String { get; set; }
        }

        public class NoArgGuyCatcher
        {
            public void Catch(NoArgGuy guy)
            {
                Guy = guy;
            }

            public NoArgGuy Guy { get; set; }
        }
        
        [Fact]
        public void no_arg_no_return_no_using_simplest_case()
        {
            var result = CodegenScenario.ForBuilds<NoArgGuy>(m =>
            {
                m.CallConstructor(() => new NoArgGuy());
                m.Return(typeof(NoArgGuy));
            });
            
            result.LinesOfCode.ShouldContain($"var noArgGuy = new {typeof(NoArgGuy).FullNameInCode()}();");
            result.Object.Build().ShouldNotBeNull();
        }
        
        [Fact]
        public void override_built_type()
        {
            var result = CodegenScenario.ForBuilds<IGuy>(m =>
            {
                m.CallConstructor(() => new NoArgGuy()).DeclaredType = typeof(IGuy);
                m.Return(typeof(NoArgGuy));
            });
            
            result.LinesOfCode.ShouldContain($"{typeof(IGuy).FullNameInCode()} noArgGuy = new {typeof(NoArgGuy).FullNameInCode()}();");
            result.Object.Build().ShouldNotBeNull();
        }
        
        [Fact]
        public void no_arg_inside_of_using_block_simplest_case()
        {
            var result = CodegenScenario.ForAction<NoArgGuyCatcher>(m =>
            {
                m.CallConstructor(() => new NoArgGuy()).Mode = ConstructorCallMode.UsingNestedVariable;
                m.Call<NoArgGuyCatcher>(x => x.Catch(null));
            });
            
            result.LinesOfCode.ShouldContain($"using (var noArgGuy = new {typeof(NoArgGuy).FullNameInCode()}())");
            
            var catcher = new NoArgGuyCatcher();
            result.Object.DoStuff(catcher);
            
            catcher.Guy.ShouldNotBeNull();
            catcher.Guy.WasDisposed.ShouldBeTrue();
        }
        
        [Fact]
        public void no_arg_return_as_built_simplest_case()
        {
            var result = CodegenScenario.ForBuilds<NoArgGuy>(m =>
            {
                m.CallConstructor(() => new NoArgGuy()).Mode = ConstructorCallMode.ReturnValue;
            });
            
            result.LinesOfCode.ShouldContain($"return new {typeof(NoArgGuy).FullNameInCode()}();");
            result.Object.Build().ShouldNotBeNull();
        }
        
        [Fact]
        public void no_arg_return_with_one_setter_case()
        {
            var result = CodegenScenario.ForBuilds<NoArgGuy, int>(m =>
            {
                var @call = m.CallConstructor(() => new NoArgGuy());
                @call.Mode = ConstructorCallMode.ReturnValue;
                @call.Set(x => x.Number);
            });
            
            result.Object.Create(11).Number.ShouldBe(11);
        }
        
        [Fact]
        public void no_arg_return_with_two_setters_case()
        {
            var result = CodegenScenario.ForBuilds<NoArgGuy, int, double>(m =>
            {
                var @call = m.CallConstructor(() => new NoArgGuy());
                @call.Mode = ConstructorCallMode.ReturnValue;
                @call.Set(x => x.Number);
                @call.Set(x => x.Double);
            });

            var noArgGuy = result.Object.Create(11, 1.22);
            noArgGuy.Number.ShouldBe(11);
            noArgGuy.Double.ShouldBe(1.22);
        }
        
        [Fact]
        public void no_arg_return_with_three_setters_case()
        {
            var result = CodegenScenario.ForBuilds<NoArgGuy, int, double, string>(m =>
            {
                var @call = m.CallConstructor(() => new NoArgGuy());
                @call.Mode = ConstructorCallMode.ReturnValue;
                @call.Set(x => x.Number);
                @call.Set(x => x.Double);
                @call.Set(x => x.String);
            });

            var noArgGuy = result.Object.Create(11, 1.22, "wow");
            noArgGuy.Number.ShouldBe(11);
            noArgGuy.Double.ShouldBe(1.22);
            noArgGuy.String.ShouldBe("wow");
        }
        
        [Fact]
        public void no_arg_return_with_three_setters_case_explicit_setter()
        {
            var result = CodegenScenario.ForBuilds<NoArgGuy, int, double, string>(m =>
            {
                var @call = m.CallConstructor(() => new NoArgGuy());
                @call.Mode = ConstructorCallMode.ReturnValue;
                @call.Set(x => x.Number);
                @call.Set(x => x.Double);
                @call.Set(x => x.String, new Value("Explicit"));
            });

            var noArgGuy = result.Object.Create(11, 1.22, "wow");
            noArgGuy.Number.ShouldBe(11);
            noArgGuy.Double.ShouldBe(1.22);
            noArgGuy.String.ShouldBe("Explicit");
        }

        public class MultiArgGuy
        {
            public int Number { get; }
            public double Amount { get; }
            public string Name { get; }

            public MultiArgGuy(int number)
            {
                Number = number;
            }

            public MultiArgGuy(int number, double amount)
            {
                Number = number;
                Amount = amount;
            }

            public MultiArgGuy(int number, double amount, string name)
            {
                Number = number;
                Amount = amount;
                Name = name;
            }
        }

        [Fact]
        public void one_argument_constructor()
        {
            var result = CodegenScenario.ForBuilds<MultiArgGuy, int>(m =>
            {
                m.CallConstructor<MultiArgGuy>(() => new MultiArgGuy(0));
                m.Return();
            });

            var guy = result.Object.Create(14);
            guy.Number.ShouldBe(14);
        }

        [Fact] 
        public void two_argument_constructor()
        {
            var result = CodegenScenario.ForBuilds<MultiArgGuy, int, double>(m =>
            {
                m.CallConstructor<MultiArgGuy>(() => new MultiArgGuy(0, 0));
                m.Return();
            });

            var guy = result.Object.Create(14, 1.23);
            guy.Number.ShouldBe(14);
            guy.Amount.ShouldBe(1.23);
            
        }
        
        [Fact] 
        public void three_argument_constructor()
        {
            var result = CodegenScenario.ForBuilds<MultiArgGuy, int, double, string>(m =>
            {
                m.CallConstructor<MultiArgGuy>(() => new MultiArgGuy(0, 0, ""));
                m.Return();
            });

            var guy = result.Object.Create(14, 1.23, "Beck");
            guy.Number.ShouldBe(14);
            guy.Amount.ShouldBe(1.23);
            guy.Name.ShouldBe("Beck");
            
        }
        
        [Fact] 
        public void override_an_argument()
        {
            var result = CodegenScenario.ForBuilds<MultiArgGuy, int, double, string>(m =>
            {
                var ctor = m.CallConstructor<MultiArgGuy>(() => new MultiArgGuy(0, 0, ""));
                ctor.Parameters[2] = new Value("Kent");
                m.Return();
            });

            var guy = result.Object.Create(14, 1.23, "Beck");
            guy.Number.ShouldBe(14);
            guy.Amount.ShouldBe(1.23);
            guy.Name.ShouldBe("Kent");
            
        }
        
        /*

         * 11. Specify some arguments
         * 13. Explicit declared type
         *
         * 
         */
    }
}