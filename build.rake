require 'albacore'
require 'albacore/tasks/versionizer'
require 'albacore/ext/teamcity'
require 'fileutils'
require 'rubygems'
require 'yaml'

task :load_config do
	@base_dir = correct_path_slashes "#{ENV['checkoutDir']}"
	@manager_dir = Paths.join @base_dir, 'BLocal.Web.Manager.MVC3'
	
	unless File.exists? Paths.join(@base_dir, 'config.yml').to_s
		puts ""
		raise "config.yml missing from root directory, fill in config.yml.template and save as config.yml to create one."
	end
	@conf = YAML.load_file("config.yml")
	
	@deploy_dir = @conf["manager"]["deploydir"]
	@libraryPaths = @conf["manager"]["customLibraries"]
end

def correct_path_slashes path
	return path.gsub("\\", "/")
end

namespace :manager do	
	nugets_restore :restore do |p|
	  p.out = Paths.join(@base_dir, "packages").to_s
	  p.exe = Paths.join(@base_dir, "tools/NuGet.exe").to_s
	end
	
	build :msbuild => [:load_config, :restore] do |b|
	  b.sln   = Paths.join @base_dir, 'BLocal.Legacy.sln'
	  b.target = ['Rebuild']
	  b.prop 'Configuration', 'Release'            
	  b.clp 'ErrorsOnly'  
	  b.be_quiet                                 
	  b.nologo
	end
	
	desc "Build the whole shazam"
	task :build => [:msbuild] do
		temp_path = Paths.join(@base_dir, "tmp_deploy").to_s;
		FileUtils.rm_rf temp_path
		FileUtils.mkdir temp_path
		
		unless File.exists? Paths.join(@deploy_dir, 'Web.config').to_s
			puts ""
			raise "Web.config missing: copy BLocal.Web.Manager.MVC3/web.config.template to the deployment directory, rename to web.config and customize."
		end
		
		FileUtils.cp_r Paths.join(@deploy_dir, 'Web.config').to_s, Paths.join(temp_path, 'Web.config').to_s
		FileUtils.rm_rf Dir.glob(Paths.join(@deploy_dir, "*").to_s)
		FileUtils.mkdir_p @deploy_dir
		FileUtils.cp_r Paths.join(temp_path, 'Web.config').to_s, Paths.join(@deploy_dir, 'Web.config').to_s
		
		FileUtils.rm_rf temp_path
		
		@libraryPaths.each do |libraryPath|
			libraryPathName = Pathname.new libraryPath
			FileUtils.cp_r libraryPath, Paths.join(@manager_dir, 'bin', libraryPathName.basename).to_s
		end
		
		FileUtils.cp_r Paths.join(@manager_dir, 'bin').to_s, Paths.join(@deploy_dir, 'bin').to_s
		FileUtils.cp_r Paths.join(@manager_dir, 'Content').to_s, Paths.join(@deploy_dir, 'Content').to_s
		FileUtils.cp_r Paths.join(@manager_dir, 'Scripts').to_s, Paths.join(@deploy_dir, 'Scripts').to_s
		FileUtils.cp_r Paths.join(@manager_dir, 'Views').to_s, Paths.join(@deploy_dir, 'Views').to_s
		FileUtils.cp_r Paths.join(@manager_dir, 'Global.asax').to_s, Paths.join(@deploy_dir, 'Global.asax').to_s		
	end
end