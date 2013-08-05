begin
  require 'bundler/setup'
  require 'fuburake'
rescue LoadError
  puts 'Bundler and all the gems need to be installed prior to running this rake script. Installing...'
  system("gem install bundler --source http://rubygems.org")
  sh 'bundle install'
  system("bundle exec rake", *ARGV)
  exit 0
end


@solution = FubuRake::Solution.new do |sln|
	sln.assembly_info = {
		:product_name => "Lightning.NET",
		:copyright => 'Copyright 2013 Ilya Lukyanov.',
		:description => '.NET-wrapper library for OpenLDAP\'s Lightning DB key-value store.'
	}

	sln.ripple_enabled = true
end