#  Обучение

1. Установить Node 
(https://nodejs.org/en/)
2. Установить .Net Core SDK 3.0.100 (https://dotnet.microsoft.com/download/dotnet-core/3.0)
3. Установить yarn 
(https://yarnpkg.com/en/docs/install#windows-stable)
3. Глобально установить библиотеку интерфейса командной строки Vue
```
$ npm i -g @vue/cli
```
4. Инициализировать серверную часть на ASP.NET Core

```
$ dotnet new api
```
5. Инициализировать клиентскую часть на Vue

```
$ vue create client-app
```

![](https://cdn.discordapp.com/attachments/467369314031239169/638763460725047296/unknown.png)

если все сделали верно то при вводе

```
$ cd client-app
$ yarn serve
```
на домене localhost поднимится приложение без backend части

![](https://cdn.discordapp.com/attachments/467369314031239169/638764923320795176/unknown.png)

6. Выдать SSL сертификат для локальной разработки

```
dotnet dev-certs https -t
```
7. Настроим сервер для SPA

```
dotnet add package Microsoft.AspNetCore.SpaServices.Extensions
```

  * В %proj_name%.csproj (рекомендуется назвать проект study) добавим настройки для окружение чтобы получилось так 

  ```
  <Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>netcoreapp3.0</TargetFramework>
    <!--      настройки для SPA    -->
    <SpaRoot>client-app\</SpaRoot>
    <DefaultItemExcludes>$(DefaultItemExcludes);$(SpaRoot)node_modules\**</DefaultItemExcludes>
    <TypeScriptCompileBlocked>true</TypeScriptCompileBlocked>
    <TypeScriptToolsVersion>Latest</TypeScriptToolsVersion>
    <IsPackable>false</IsPackable>
    <!--       /настройки для SPA    -->

  </PropertyGroup>
 <!--      настройки для SPA    -->
  <Target Name="PublishRunWebpack" AfterTargets="ComputeFilesToPublish">
    <Exec WorkingDirectory="$(SpaRoot)" Command="yarn" />
    <Exec WorkingDirectory="$(SpaRoot)" Command="yarn build" />
    <ItemGroup>
      <DistFiles Include="$(SpaRoot)dist\**" />
      <ResolvedFileToPublish Include="@(DistFiles->'%(FullPath)')" Exclude="@(ResolvedFileToPublish)">
        <RelativePath>%(DistFiles.Identity)</RelativePath>
        <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
      </ResolvedFileToPublish>
    </ItemGroup>
  </Target>
  <!--      /настройки для SPA    -->

  <ItemGroup>
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.SpaServices.Extensions" Version="3.0.0" />
  </ItemGroup>
  <!--      настройки для SPA    -->
  <ItemGroup>
    <Content Remove="$(SpaRoot)**" />
    <None Remove="$(SpaRoot)**" />
    <None Include="$(SpaRoot)**" Exclude="$(SpaRoot)node_modules\**" />
  </ItemGroup>

  <Target Name="DebugEnsureNodeEnv" BeforeTargets="Build" Condition=" '$(Configuration)' == 'Debug' And !Exists('$(SpaRoot)node_modules') ">
    <Exec Command="node --version" ContinueOnError="true">
      <Output TaskParameter="ExitCode" PropertyName="ErrorCode" />
    </Exec>
    <Error Condition="'$(ErrorCode)' != '0'" Text="Node.js is required to build and run this project. To continue, please install Node.js from https://nodejs.org/, and then restart your command prompt or IDE." />
    <Message Importance="high" Text="Restoring dependencies using 'npm'. This may take several minutes..." />
    <Exec WorkingDirectory="$(SpaRoot)" Command="npm install" />
  </Target>
  <!--      /настройки для SPA    -->

</Project>
```

  * В папке проекта создадим файл "VueConnection.cs" с классом Connection который опишет сопряжение с клиентским приложением в режиме разработки. Необходимо наполнить этот файл:
  ```
  using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Study.VueConnection
{
    public static class Connection
    {

        private static int Port { get; } = 8080;
        private static Uri DevelopmentServerEndpoint { get; } = new Uri($"http://localhost:{Port}");
        private static TimeSpan Timeout { get; } = TimeSpan.FromSeconds(60);

        private static string DoneMessage { get; } = "DONE  Compiled successfully in";

        public static void UseVueDevelopmentServer(this ISpaBuilder spa)
        {
            spa.UseProxyToSpaDevelopmentServer(async () =>
            {
                var loggerFactory = spa.ApplicationBuilder.ApplicationServices.GetService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("Vue");

                if (IsRunning())
                {
                    return DevelopmentServerEndpoint;
                }


                var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                var processInfo = new ProcessStartInfo
                {
                    FileName = isWindows ? "cmd" : "npm",
                    Arguments = $"{(isWindows ? "/c npm " : "")}run serve",
                    WorkingDirectory = "client-app",
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                };
                var process = Process.Start(processInfo);
                var tcs = new TaskCompletionSource<int>();
                _ = Task.Run(() =>
                {
                        try
                        {
                            string line;
                            while ((line = process.StandardOutput.ReadLine()) != null)
                            {
                                logger.LogInformation(line);
                                if (!tcs.Task.IsCompleted && line.Contains(DoneMessage))
                                {
                                    tcs.SetResult(1);
                                }
                            }
                        }
                        catch (EndOfStreamException ex)
                        {
                            logger.LogError(ex.ToString());
                            tcs.SetException(new InvalidOperationException("'npm run serve' failed.", ex));
                        }
                    });
                _ = Task.Run(() =>
                {
                        try
                        {
                            string line;
                            while ((line = process.StandardError.ReadLine()) != null)
                            {
                                logger.LogError(line);
                            }
                        }
                        catch (EndOfStreamException ex)
                        {
                            logger.LogError(ex.ToString());
                            tcs.SetException(new InvalidOperationException("'npm run serve' failed.", ex));
                        }
                    });

                var timeout = Task.Delay(Timeout);
                if (await Task.WhenAny(timeout, tcs.Task) == timeout)
                {
                    throw new TimeoutException();
                }

                return DevelopmentServerEndpoint;
            });

        }

        private static bool IsRunning() => IPGlobalProperties.GetIPGlobalProperties()
                .GetActiveTcpListeners()
                .Select(x => x.Port)
                .Contains(Port);
    }
}
  ```

  * Дополним файл Startup.cs функциями раннее написанного класса. Файл должен получиться таким:
  ```
  using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Study.VueConnection;


namespace Study
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            // настройка для продакшена (где находится стаатичный билд)
            services.AddSpaStaticFiles(options => options.RootPath = "client-app/dist");

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }


            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            //настройка миддлвари для поднятия спа при старте
            app.UseSpaStaticFiles();
            app.UseSpa(
                spa =>
                {
                    spa.Options.SourcePath = "client-app";
                    if (env.IsDevelopment())
                    {
                        spa.UseVueDevelopmentServer();
                    }
                }
            );

        }
    }
}
  ```
7. Установим HTTP клиент
```
$ cd client-app/
$ yarn add axios
```
8. Переходим в main.js, который находится в client-app/src, и подключаем axios, по итогу файл должен выглядеть так:
```
import Vue from 'vue'
import App from './App.vue'
import router from './router'
import axios from 'axios'

Vue.prototype.$axios = axios

Vue.config.productionTip = false

new Vue({
  router,
  render: h => h(App)
}).$mount('#app')

```
На этом настройка рабочего окружения выполнена
можно приступать к разработке.

10. Добавляем библиотеку SqlServer - фреймворк для Баз Данных
```
$ dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```
11. Добавляем библиотеку InMemory - надстройка для иммитации Баз Данных в разработке
```
$ dotnet add package Microsoft.EntityFrameworkCore.InMemory
```
12. Создаем модель OrderModel.cs в папке Models, и наполняем его так чтобы получилось:
```
namespace Study.Models
{

    public class Order
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string ProductName { get; set; }
    }
}
```

13. Создаем контекст базы данных в виде файла Context.cs в папке Database, и наполняем его так чтобы получилось:
```
using Microsoft.EntityFrameworkCore;
using Study.Models;


namespace Study.DataBase
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
    }
}
```
14. Настраеваем файл Startup.cs, который находится в папке проекта, чтобы по итогу он выглядел так (ранее мы уже работали с ним):
```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Study.DataBase;

using Study.VueConnection;


namespace Study
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //настройка базы данных
            services.AddDbContext<Context>(opt =>
               opt.UseInMemoryDatabase("Orders"));

            services.AddControllers();
            // настройка для продакшена (где находится стаатичный билд)
            services.AddSpaStaticFiles(options => options.RootPath = "client-app/dist");

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }


            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            //настройка миддлвари для поднятия спа при старте
            app.UseSpaStaticFiles();
            app.UseSpa(
                spa =>
                {
                    spa.Options.SourcePath = "client-app";
                    if (env.IsDevelopment())
                    {
                        spa.UseVueDevelopmentServer();
                    }
                }
            );

        }
    }
}
```

15. Установим библиотеки для Скафолдинга - автогенирация кода
```
$ dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
$ dotnet add package Microsoft.EntityFrameworkCore.Design
$ dotnet tool install --global dotnet-aspnet-codegenerator
```

16. Сгенерируем контроллер управления ордерами
```
$ dotnet aspnet-codegenerator controller -name OrdersController -async -api -m Order -dc Context -outDir Controllers
```

17. Создадим новую страницу Vue. Для этого создадим файл AllOrders в папке views, которая находится в папке client-app/src, и наполним его так чтобы получилось:
```
<template>
  <div class="AllOrders">
    <table>
      <tbody>
        <tr>
          <td>ID</td>
          <td>Client Name</td>
          <td>Product Name</td>
        </tr>
        <tr v-for="order in orders" :key="order.id">
          <td>{{order.id}}</td>
          <td>{{order.clientName}}</td>
          <td>{{order.productName}}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script>
export default {
  data: () => ({
    orders: []
  }),
  created() {
    this.$axios.get("https://localhost:5001/api/orders").then(res => {
      this.orders = res.data;
      console.log(res);
    });
  }
};
</script>
```
18. Создадим новую страницу Vue. Для этого создадим файл NewOrder в папке views, которая находится в папке client-app/src, и наполним его так чтобы получилось:
```
<template>
  <div class="NewOrder">
    <!-- <h1>This is an new order page</h1>
    <button @click="sendOrder">отправить ордер</button>
    -->
    <form @submit.prevent="sendOrder">
      <div class="input-group">
        <lable class="input-lable">Ваше имя</lable>
        <input type="text" class="input-input" v-model="order.clientName" />
      </div>
      <div class="input-group">
        <lable class="input-lable">Продукт</lable>
        <input type="text" class="input-input" v-model="order.productName" />
      </div>
      <button type="submit">отправить</button>
    </form>
    <h3>{{message}}</h3>
  </div>
</template>

<script>
export default {
  data: () => ({
    message: "",
    order: {
      clientName: "",
      productName: ""
    }
  }),
  methods: {
    sendOrder() {
      this.$axios
        .post("https://localhost:5001/api/orders", this.order, {
          headers: {
            "Content-Type": "application/json"
          }
        })
        .then(res => {
          console.log(res.data);
          this.message =
            "продукт " + res.data.productName + " успешно добавлен";
          this.order = {
            clientName: "",
            productName: ""
          };
        });
    }
  }
};
</script>
```

19. Подключим новые файлы Vue в файле index.js, который находится в папке client-app/src/router, так чтобы получилось:
```
import Vue from 'vue'
import VueRouter from 'vue-router'
import Home from '../views/Home.vue'

Vue.use(VueRouter)

const routes = [
  {
    path: '/',
    name: 'home',
    component: Home
  },
  {
    path: '/about',
    name: 'about',
    // route level code-splitting
    // this generates a separate chunk (about.[hash].js) for this route
    // which is lazy-loaded when the route is visited.
    component: () => import('../views/About.vue')
  },
  {
    path: '/newOrder',
    name: 'New Order',
    component: () => import('../views/NewOrder.vue')
  },
  {
    path: '/allOrders',
    name: 'all Orders',
    component: () => import('../views/AllOrders.vue')
  }
]

const router = new VueRouter({
  routes
})

export default router
```

1.  Добавим ссылки на эти страницы Vue в меню в главном layout файле App.vue, который находится в папке client-app/src, так чтобы получилось:
```
<template>
  <div id="app">
    <div id="nav">
      <router-link to="/">Home</router-link>|
      <router-link to="/about">About</router-link>|
      <router-link to="/neworder">new order</router-link>
      <router-link to="/allorders">all orders</router-link>
    </div>
    <router-view />
  </div>
</template>

```
-----

после всех манипуляций, прописав команду `dotnet run ` на https://localhost:5001/#/ поднимется приложение. нажав на ссылку "all orders" мы попадем на страницу со всеми заказами.